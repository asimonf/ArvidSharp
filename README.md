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

# Usage on BeagleBone

This requires an image based on, at least, Debian 9. The newer, the better I guess. Given that things changed between
kernel 3.8.x and the kernel 4.x (or newer), things have to be done a little differently compared to libarvid. First, 
make sure that the rproc is [blacklisted](http://catch22.eu/beaglebone/beaglebone-pru-uio/). Secondly, you must config
the uio module given that it will be loaded automatically. To do that, create a file called `/etc/modprobe.d/uio_pruss-options.conf`. 
In that file, add the following:

```
options uio_pruss extram_pool_sz=0x400000
```

After this. Just run the server binary as a root user (required, I think).

I'll improve docs as things move forward. 