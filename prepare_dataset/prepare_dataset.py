from PIL import Image
import os

img_folder = "/Users/lingdonghuang/Downloads/leftImg8bit_trainvaltest/leftImg8bit/train"
ann_folder = "/Users/lingdonghuang/Downloads/gtFine_trainvaltest/gtFine/train"

def smart_resize(im,w,h):
    iw,ih = im.size
    nim = None
    # 1 +-+---+-+ 2 +---+
    #   | |   | |   +---+
    #   | |   | |   |   |
    #   +-+---+-+   +---+
    #               +---+
    if iw*1.0/ih >= w*1.0/h:            
        nw = ih * w*1.0 / h
        padx = (iw - nw)/2.0
        nim = im.crop(box=(padx,0,iw-padx,ih))

    else:
        nh = iw * h*1.0 / w
        pady = (ih - nh)/2.0
        nim = im.crop(box=(0,pady,iw,ih-pady))

    nim = nim.resize((w,h))
    return nim


if __name__ == "__main__":
    cities = os.listdir(img_folder)

    for i in range(len(cities)):
        fnames0 = os.listdir(img_folder+"/"+cities[i])
        fnames1 = map((lambda x: "_".join(x.split("_")[:-1])+"_gtFine_color.png"), fnames0)

        for j in range(0,len(fnames0)):
            f0 = img_folder+"/"+cities[i]+"/"+fnames0[j]
            f1 = ann_folder+"/"+cities[i]+"/"+fnames1[j]
            im0 = Image.open(f0)
            im1 = Image.open(f1)
            im0 = smart_resize(im0,512,512)
            im1 = smart_resize(im1,512,512)
            im = Image.new('RGB',(im0.size[0]+im1.size[0],im0.size[1]))
            im.paste(im0,(0,0))
            im.paste(im1,(im0.size[0],0))
            #im.show()
            fname = "_".join(fnames0[j].split("_")[:-1])
            im.save("out/"+fname+".png","PNG")

        