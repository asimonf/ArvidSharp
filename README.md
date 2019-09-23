# ArvidSharp

This is a reimplementation of [libarvid](https://github.com/ole00/libarvid) in C# with a few small changes that I think 
could help with both reliability and performance.

* Separate the control communication from video streaming using TCP for the first and UDP for the second.
* Use the ZStandard compression library instead of zlib. This should help with heavier images and with performance on both sides.

I'm also looking for ways to remove double-buffering and add proper vsync timing monitoring to implement a real virtual vsync.

This is a WIP and is not in any way usable right now. 
