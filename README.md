# RS9000
RS9000 is a realistic traffic enforcement radar system for FiveM servers.

***Warning:** RS9000 is a **work in progress**. Currently there is no guarantee on the stability of the software. All features are subject to change.*

![Radar](https://i.imgur.com/hP7B0E0.png)

## Features

* An elegant and **realistic look**
* **Dual antennas** (front and rear)
* **Multi-mode** antennas

## Usage

Control of the radar system is done using the control panel.

* Press **Ctrl+X** to open the control panel

## Installation

To install RS9000 on your FiveM server, follow these simple steps:
* Download the [latest version](https://github.com/randomsean/RS9000/releases)
* Unpack the `rs9000` directory inside the `.zip` file to your FiveM server `resources` directory
* Add the following directive to your FiveM `server.cfg` file
```
ensure rs9000
```
* That's it! Well, unless you wish to [configure](#configuration) it

## Configuration

Configuration occurs in the `config.json` file included in the release zip. Please make sure this file exists and has the default values  even if you do not wish to make configuration changes.

The following is the currently supported configuration keys and values:

| Key                                  | Values            | Default | Notes                                 |
| ------------------------------------ | ----------------- | ------- | ------------------------------------- |
| `units`                              | `"mph"`, `"km/h"` | `"mph"` | Units of speed displayed on the radar |
| `plateReader`                        | `true`/`false`    | `true`  | Plate reader functionality            |
| `beep`                               | `true`/`false`    | `true`  | Whether or not beep is on by default  |
| `fastLimit`                          | `0-999`           | `80`    | Default fast speed limit              |
| `controls.openControlPanel.modifier` | Control index     | `224`   | `INPUT_SCRIPT_RLEFT` or `LEFT CTRL`   |
| `controls.openControlPanel.control`  | Control index     | `73`    | `INPUT_VEH_DUCK` or `X`               |

#### Controls

To configure controls, use the control index values found [here](https://docs.fivem.net/game-references/controls/). To disable the use of a control or modifier, set its index to `-1`.

## Contribute

Contributions welcome! Please submit an issue or pull request to report and/or fix a bug.

If you would like to suggest a feature, please open an issue with the *enhancement* label to start a discussion.

## License

RS9000 is licensed under the [MIT license](https://github.com/randomsean/RS9000/blob/master/LICENSE).

The in-game interface uses the [DSEG](https://www.keshikan.net/fonts-e.html) and [D-DIN](https://www.datto.com/fonts/d-din/) fonts. Both fonts are licensed under the [SIL Open Font License 1.1](https://scripts.sil.org/OFL).

*This project is heavily based on the fantastic **WraithRS** project by **WolfKnight**. Thanks, WolfKnight, for the inspiration!*
