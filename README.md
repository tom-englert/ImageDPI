# ImageDPI
View and fix DPI settings of PNG images

WPF is a DPI aware framework, thus the DPI settings in images are important for rendering.

Having bitmaps with the intended pixel size, but a resolution which is not 96dpi, 
will cause unwanted scaling or blurriness.

Especially in PNG images the resolution is not specified in DPI, but some unit per meter, 
so you never can specify exactly 96dpi. 

Having the 95.9866...dpi is actually no longer issue with WPF, but other resolutions can still cause problems, 
requiring unnecessary complex styles to fix the display.

To use the native WPF resolution, you must remove the DPI tag from the PNG image, 

This tool analyzes all PNG images and visualizes them, with an option to fix (i.e. remove) the 
DPI information from the image.
