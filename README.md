# ArvidSharp

This is a reimplementation of [libarvid](https://github.com/ole00/libarvid) in C# with a few small changes that I think 
could help with both reliability and performance.

* Separate the control communication from video streaming using TCP for the first and UDP for the second.
* Use the ZStandard compression library instead of zlib. This should help with heavier images and with performance on both sides.

I'm also looking for ways to remove double-buffering and add proper vsync timing monitoring to implement a real virtual vsync.

This is a WIP and is not in any way usable right now. 

This is provided as is without warranty and, as such, you should use this at your discretion. 

I must also add licencing notices, but just in case, I don't claim ownership of anything done by either TI or Marek 
(original creator of libarvid and arvid-client). The pasm executable was built by me and is included for ease of use.

This project is derived from Marek's work and it wouldn't be possible without his help. 