# osu-catch-Difficulty-Visualizer
Simple tool made to display an osu!catch beatmap and its difficulty attributes.

Supports .osu files parsing only for osu!catch. osu!standard beatmaps are converted.
Supports HR, DT and EZ mods. Circle Size is supported but Approach Rate is not. You can scroll through a beatmap using your mouse wheel or a slider.

This tool comes bundled with the actual osu! source code (though not updated but working perfectly fine for our use case). See [the osu! Github](https://github.com/ppy/osu).

This is meant to help for the osu!catch performance points developments. It can be used as a tool for mapping but be aware that it might not display a perfect representation of every beatmap.
Also, since I had no idea how to develop a proper WPF application before starting this project, you will notice that the user interface and the UI in general is pretty bad. I would greatly appreciate a PR to make things a little bit better :sweat_smile:

In case something goes wrong, you can PM me on Discord: bastoo0#7314

But remember that if something is broken, it's not a bug, it's a feature :wink:

Shout out to Ttobas for these hours of debugging at 2AM :smiley:

# For developers
You can obviously contribute to the source code. You will need to have at least .NET Core 5.0 installed and Visual Studio 2019 (not an earlier version) to open the solution.

Later, I'm going to implement a way to have both the current SR calculation and an experimental calculation (for PP changes) displayed at the same time. This will only be available via this source code though.

# Download
You can download the program installer here: [Windows(x86)(.msi)](https://mega.nz/file/Nm5BXC4I#UbY2nmFWf3jQxxYU0Mxrdq8OeWWYZIupEs6ZHAAD4VI)

## License
[MIT](https://choosealicense.com/licenses/mit/)
