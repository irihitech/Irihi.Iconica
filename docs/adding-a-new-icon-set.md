# Adding a new icon set

Step-by-step SOP for wiring a new upstream icon set into Irihi.Iconica.
Read `AGENTS.md` first for the hard rules (generated code, submodules, idempotency).

**TDesign is the reference implementation** — keep its files open alongside this
document: `src/IconGenerator.TDesign/`, `src/Irihi.Iconica.TDesign/`,
`demo/Irihi.Iconica.Demo/` (search for "TDesign").

## 0. Decide the mapping before writing code

Answer these from the upstream sources first; they drive every later step:

- **Source layout**: where the raw SVGs live in the upstream repo, and where
  icon metadata (categories/keywords) comes from. Expect gaps and name
  mismatches in the metadata (e.g. Semi's `meta.json` calls an icon
  `a_i_bell_level_1` while the SVG file is `ai_bell_level_1.svg`) — normalize.
- **License**: needed for the `NOTICE` file. Upstream must be redistributable
  (MIT/Apache-2.0 style); record upstream repo URL and license.
- **Naming**: file name → C# class name. PascalCase split on the set's
  separators (`-` for TDesign, `_` and stray `-` for Semi); prefix class names
  starting with a digit with `_`. Check for collisions.
- **Color model → brush slots**: how upstream `fill`/`stroke` values map to the
  slots exposed by your `<Set>IconBase`. Typical mappings:
  - `black` / theme-color fills → slot 0 (foreground, inherit `TextElement.Foreground`)
  - brand accent colors / two-tone second color → dedicated StyledProperty slots
  - fixed multi-color palettes or gradients (e.g. Semi lab / AI icons) need
    per-element fixed brushes or a dedicated gradient slot — extend
    `DrawingElement`/Core only if the slot model cannot express it
  - `stroke` attributes → `StrokeIndex` slots with per-set defaults for
    `StrokeWidth`/`LineCap`/`LineJoin`
- **viewBox**: usually `0 0 24 24`; if any upstream SVG deviates, the icon base
  must scale by the per-icon viewBox instead of a hardcoded 24.
- **Structure quirks**: `fill-rule="evenodd"` paths (verify rendering),
  `<defs>` clipPaths (usually ignorable Figma export noise), transforms.

## 1. Add the submodule

```bash
git submodule add git@github.com:<org>/<upstream-repo>.git external/<name>
```

Existing entries in `.gitmodules` use SSH URLs. Submodules are read-only inputs.

## 2. Create `src/IconGenerator.<Set>`

Console app, `<TargetFrameworks>net8.0;net10.0</TargetFrameworks>`. Copy the
skeleton from `IconGenerator.TDesign` (SVG + XML parsing) or `IconGenerator.IconPark`
(TSX + JSON). Non-negotiable behaviors (also listed in AGENTS.md):

- Repository root: optional `--root <path>` argument; default is five levels up
  from the build output directory.
- **Delete and recreate** output directories before writing, so icons removed
  upstream never linger.
- **Fail fast**: collect per-icon failures, print them plus a summary to
  stderr, `Environment.Exit(1)`.
- **Idempotent output**: single-line header (no dates, no copyright years —
  attribution belongs in `NOTICE`), deterministic ordering.
- Outputs:
  - `src/Irihi.Iconica.<Set>/Generated/**/<Class>.cs`
  - `demo/Irihi.Iconica.Demo/Models/IconInfo.<Set>.Generated.cs`
    (partial `IconInfo`, chunked into `Create<Set>Chunk_XXXX()` methods of 100)

## 3. Create the package `src/Irihi.Iconica.<Set>`

Copy `Irihi.Iconica.TDesign` and adapt:

- `Irihi.Iconica.<Set>.csproj` — PackageId, version, icon/license/readme/NOTICE
  packing entries (see TDesign csproj), `ProjectReference` to Core.
- `Core/<Set>IconBase.cs` — abstract `Control`: StyledProperties for the brush
  slots decided in step 0, `StrokeWidth`/`LineCap`/`LineJoin` defaults that
  match the upstream stroke style, `Width`/`Height` default 24, and a `Render`
  override that scales by the viewBox and draws `DrawingData` via
  `DrawingContextExtensions`.
- `Core/IconMode.cs` (optional) — if the set supports switchable color modes.
- `GlobalUsing.cs`, `Generated/` (from step 2), `README.md`.
- `NOTICE` — upstream name, URL, copyright, license.
- `Properties/AssemblyInfo.cs` — `XmlnsDefinition`/`XmlnsPrefix`
  `https://irihi.tech/iconica/<set>` → namespace `Irihi.Iconica.<Set>` and
  `Irihi.Iconica.<Set>.Icons`.

## 4. Wire up the demo

- `ViewModels/<Set>PageViewModel.cs` — composes `IconViewerViewModel`,
  `IconDetailViewModel`, `<Set>SettingPanelViewModel` (copy TDesign's).
- `ViewModels/<Set>SettingPanelViewModel.cs` — exposes the set's brush/stroke
  properties; implements `IExportSettings`; registers theme-change messages on
  the set's messenger channel.
- `Views/Parts/<Set>Page.axaml(.cs)` and `<Set>SettingPanelView.axaml(.cs)`.
- `MainWindowViewModel` — add the page property, extend `PageNames`, visibility
  flags, and the `SelectedTabIndex` / export / detail plumbing. NOTE: this
  class currently branches `if/else` on `SelectedTabIndex` for two sets;
  extend it (or refactor to a collection) rather than duplicating blindly.
- `MainWindow.axaml` — add the tab header/content if it enumerates pages.

## 5. Solution and CI

- Add the two new projects to `Irihi.Iconica.slnx` (generators at the top
  level, packages beside the existing ones).
- `.github/workflows/pack.yml` — add a boolean input `Irihi_Iconica_<Set>` and
  a `dotnet pack src/Irihi.Iconica.<Set> -o nugets` step gated on it.
- `.github/workflows/release-tag.yml` — pass the new pack option through if it
  enumerates packages.
- Update `README.md` and `README.zh-CN.md` (install section, usage, repository
  layout tree) — keep both languages in sync.

## 6. Verify

1. Run the generator **twice**; the second run must produce zero diff.
2. `dotnet build Irihi.Iconica.slnx` — 0 errors.
3. `dotnet run --project demo/Irihi.Iconica.Demo.Desktop` — browse the new
   tab; spot-check stroke vs fill variants, and the odd icons identified in
   step 0 (gradients, fixed colors, non-24 viewBox, evenodd paths).
4. Confirm `git status` contains only intended changes (no submodule dirt, no
   stray generated leftovers).
