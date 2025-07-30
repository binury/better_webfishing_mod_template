# (Better) Webfishing Mod Template

A (better) example of [GDWeave](https://github.com/NotNite/GDWeave) Webfishing mods.

## Project overview

This repository is a developer-friendly replacement for the _old_ vanilla one that Nite
threw together for GDWeave. It includes a *much needed* abstraction layer for working with
GDWeave's [Tokens](), written primarily by [Teemaw](https://teemaw.dev) but until now
never exported and made available for reuse in other projects. Without this, mods/patches
need to be written in tedium, an error-prone and monotonous way:

```cs
{
    internal class PlayerScriptMod : IScriptMod
    {
        public bool ShouldRun(string path) => path == "res://Scenes/Entities/Player/player.gdc";
        public IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens)
        {
            // scale = clamp(animation_data["player_scale"], 0.6, 1.4) * Vector3.ONE
            var clampMatcher = new MultiTokenWaiter([
                t => t is IdentifierToken { Name: "scale" },
                t => t.Type is TokenType.OpAssign,
                t => t.Type is TokenType.BuiltInFunc, // clamp
                t => t.Type is TokenType.ParenthesisOpen,
                t => t is IdentifierToken { Name: "animation_data" },
                t => t.Type is TokenType.BracketOpen,
                t => t.Type is TokenType.Constant, // player_scale
                t => t.Type is TokenType.BracketClose,
                t => t.Type is TokenType.Comma,
                t => t.Type is TokenType.Constant, // 0.6
            ], allowPartialMatch: false);
            foreach (var token in tokens)
            {
                if (clampMatcher.Check(token))
                {
                    // -2.0, 10.0
                    yield return new ConstantToken(new RealVariant(-2.0));
                    yield return new Token(TokenType.Comma);
                    yield return new ConstantToken(new RealVariant(10.0));
                }
                // [...]
                else
                {
                    // return the original token
                    yield return token;
                }
            }
        }
    }
}
```

Instead this mod could be rewritten with as:
```cs
mi.RegisterScriptMod(
			new TransformationRuleScriptModBuilder()
				.ForMod(psmod)
				.Named("Clamp")
				.Patching("res://Scenes/Entities/Player/player.gdc")
				.AddRule(
					new TransformationRuleBuilder()
						.Named("Expand clamp range")
						.Do(Operation.Append)
						.Matching(
							TransformationPatternFactory.CreateGdSnippetPattern(
								"""
								scale = clamp(animation_data["player_scale"],
								"""
							)
						)
						.With("-2.0, 10.0")
                    )
                // [...]
				.Build()
		)
```

## Examples in this project

I tried to include _many_ example patches in a variety of styles to showcase as much as I could without potentially overwhelming
the reader. For what it's worth, a typical mod might never get this large so don't feel like you've done something wrong if your mod
file ends up being only 50 or so lines-- that's totally normal!
file ends up being only 50 or so lines-- that's totally normal!

## Other examples to reference
- [BetterLocalChat](https://github.com/binury/Toes.BetterLocalChat)
- [Calico](https://github.com/tma02/calico/tree/main)
- Submit a PR if you know of a good project to add here!

## Building

To build the project, you need to set the GDWeavePath environment variable to your game install's GDWeave directory
(e.g. G:\games\steam\steamapps\common\WEBFISHING\GDWeave).
This can also be done in Rider with File | Settings | Build, Execution, Deployment | Toolset and Build | MSBuild global properties.
You can also achieve this with a .user file, in Visual Studio.

## Publishing your finished mod

See included Powershell script publish.ps1