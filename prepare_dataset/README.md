# Prepare Dataset
## Contents

### `prepare_dataset.py`

Prepare images from cityscapes dataset for pix2pix training.

### `batch.py`

Collection of image utilities for pix2pix & pix2pixHD testing. e.g.

- resize images
- duplicate images side by side to pix2pix testing format
- combine pix2pix inputs and results into single image

etc.

### `lbl_ins_map.py`

Produce label maps and instance maps from colorful semantic image (for pix2pixHD). The instance maps can be placed under `/datasets/cityscapes/test_inst`, the label maps can be placed under `/datasets/cityscapes/test_label`. Delete the folders original content as needed.

Takes 70 seconds to process one low quality 1080p image extracted from video by ffmpeg on MacBook Pro. Takes 5 seconds to process high quality 1080p screenshot.


## ffmpeg Commands


### Extract frames from video
`ffmpeg`
+

options | effects
-----|-----
`-i inp.mov ` |  Input video file name
`-vf fps=5/1 ` | 5 frames per 1 second
`input_dir/%05d.jpg ` | Output image files regex



### Combine frames into video
`ffmpeg`
+

options | effects
-----|-----
`-r 10 -f image2 ` |  10 FPS
`-s 2048x1024 ` | Resolution
`-start_number 1 ` | Start index (of file names)
`-i output_dir/images/%05d-outputs.png ` | Input files regex
`-vcodec libx264 -crf 25  -pix_fmt yuv420p ` | codec (?)
`test.mp4` | Output video file name

## pix2pixHD Fixes

- Change `default=50` in line `13` of `pix2pixHD/options/test_options.py` to `default=50000` so pix2pixHD doesn't exit after only processing the first 50 images.
- To test with CPU, add `--gpu_ids -1` to the command and search for `.cuda()` and `.cuda` in all source files and replace with empty string.

