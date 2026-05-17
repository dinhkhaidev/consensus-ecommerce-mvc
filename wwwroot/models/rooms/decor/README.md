# Room3D Signature Decor Assets

Place optimized web-ready GLB files in this folder:

- `shenron-yanez.glb`
- `katana-wall.glb`

Alternatively, run `scripts/download-room3d-sketchfab-decor.ps1` with a Sketchfab API token. The script downloads Sketchfab glTF zips and extracts them to:

- `shenron-yanez/scene.gltf`
- `katana-wall/scene.gltf`

The Room3D loader checks both the `.glb` path and the extracted `scene.gltf` path.

## Shenron

Planned source:

- Sketchfab: Shenron by Yanez Designs
- URL: https://sketchfab.com/3d-models/shenron-dragon-ball-96e8ad1e206941ce859c5733bec75d30
- License shown during planning: CC Attribution

Recommended optimization before shipping:

- Target about 10k-15k triangles for the fixed room sculpture.
- Prefer 1K-2K texture maps.
- Export as binary glTF (`.glb`).

Important: Shenron is a Dragon Ball character. For public commercial usage, confirm rights/license fit your use case.

## Katana

Current included source:

- Poly Haven: Antique Katana 01
- URL: https://polyhaven.com/a/antique_katana_01
- License: CC0
- Installed path: `katana-wall/scene.gltf`
- Installed texture tier: 2K

Earlier planned source:

- Sketchfab: Katana by Superior
- URL: https://sketchfab.com/3d-models/katana-aec45be9d79d4c0885c2ec7fb106fd4b
- License shown during planning: CC Attribution
- Reported size: about 9.4k triangles, 4.9k vertices

Recommended optimization before shipping:

- Export as `katana-wall.glb`.
- Keep the model around 6k-10k triangles.
- Add/keep a subdued wall rack so it reads as a mounted decor piece.
- Use metal/roughness materials for the blade and dark wood or black material for the rack.

The Room3D page includes geometry fallbacks for both assets, so the scene still renders if these GLB files are not present.
