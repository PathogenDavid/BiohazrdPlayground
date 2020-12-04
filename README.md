# Biohazrd Playground

[![MIT Licensed](https://img.shields.io/github/license/pathogendavid/biohazrdplayground?style=flat-square)](LICENSE.txt)
[![Sponsor](https://img.shields.io/badge/sponsor-%E2%9D%A4-lightgrey?logo=github&style=flat-square)](https://github.com/sponsors/PathogenDavid)

This repo contains a quick-and-dirty tool that I used to generate the declaration trees and sample outputs for the [Biohazrd documentation](https://github.com/InfectedLibraries/Biohazrd/). (The [`LiftAnonymousUnionFieldsTransformation`](https://github.com/InfectedLibraries/Biohazrd/blob/4ced5521b64ac4e60d42c8ffcad14c72e324fd06/docs/BuiltInTransformations/LiftAnonymousUnionFieldsTransformation.md#details) documentation is a good example of the sorts of things this tool outputs.)

The tool is not particularly friendly. You need to modify [`Program.cs`](BiohazrdPlayground/Program.cs) to switch between examples.

This tool is using Biohazrd in pretty unusual ways, so I don't consider it best practice for interacting with Biohazrd in typical generator scenarios. (The [first-party Biohazrd-generated libraries](https://github.com/InfectedLibraries/?q=Infected&type=source) are better for that purpose.)

## License

This project is licensed under the MIT License. [See the license file for details](LICENSE.txt).

Additionally, this project has some third-party dependencies. [See the third-party notice listing for details](THIRD-PARTY-NOTICES.md).
