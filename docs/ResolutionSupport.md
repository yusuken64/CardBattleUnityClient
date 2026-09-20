# Resolution support

The authored game area is 1920 × 1080. Screen-space canvases use Scale With Screen Size and Expand. `GameCanvasViewport` maps their direct children's anchors into a centered 16:9 viewport without adding a layout wrapper. `GameViewportController` uses that same viewport for game cameras and draws input-blocking bars outside it.

Ordinary screen-space UI has its scale baked into rectangle dimensions, text, image metrics, and layout settings. World-space cards and animated card actors retain their intentional scales. Normalized runtime prefab variants preserve the sizes previously supplied by scaled spawning containers. Intro timeline scale curves are adjusted to the normalized base sizes.

Resolution settings save width and height rather than a dropdown index. Windowed mode supports common window sizes; exclusive fullscreen chooses a reported monitor mode; borderless follows the desktop. Pointer handling rejects interactions beginning outside the game area, cancels drops in the bars, and scales its drag threshold. Popup bounds use their actual screen-space dimensions.

## Validation

`ResolutionLayoutTests` covers viewport geometry, resolution choices, persistence across reordered choices, and popup clamping. Managed invocation passes all 13 cases. Additional migration checks compared 1,388 serialized UI rectangles against pre-migration snapshots at 1080p; the largest difference was below 0.00003 pixels. These checks do not simulate Unity layout rebuilding or gameplay.

Before release, run Unity play-mode checks at 1280 × 720, 1920 × 1080, 3840 × 2160, 1920 × 1200, and 3440 × 1440. Check main menus, deck building, adventure, battle/tutorial, pack opening, both intros, and StoryMode. Exercise scrolling, card selection, dragging, targeting, tooltips near every edge, settings reopening, scene transitions, and resizing during interaction. Confirm visual readability at 720p and that clicks in the bars cannot trigger gameplay. Play-mode visual validation has not yet been completed.
