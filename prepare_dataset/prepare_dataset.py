from PIL import Image
import os
import sys

img_folder = "../../leftImg8bit_trainvaltest/leftImg8bit/train"
ann_folder = "../../gtFine_trainvaltest/gtFine/train"

print "-- Dataset Preparation --"

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

    img_total = 0;
    for i in range(len(cities)):
        img_total += len(os.listdir(img_folder+"/"+cities[i]))
    print str(img_total)+" images across "+str(len(cities))+" folders will be processed.\n"

    maxphotos = raw_input("Max photos (null=all): ")
    maxphotos = int(maxphotos) if len(maxphotos) else img_total
    
    height = raw_input("Height (null=512): ")
    height = int(height) if len(height) else 512
    width = raw_input("Width (null=512): ")
    width = int(width) if len(width) else 512

    counter = 0
    for i in range(len(cities)):
        fnames0 = os.listdir(img_folder+"/"+cities[i])
        fnames1 = map((lambda x: "_".join(x.split("_")[:-1])+"_gtFine_color.png"), fnames0)

        if(counter > maxphotos): break
        print "\nProcessing /" + cities[i]

        for j in range(0,len(fnames0)):
            counter += 1
            if(counter > maxphotos): break
            f0 = img_folder+"/"+cities[i]+"/"+fnames0[j]
            f1 = ann_folder+"/"+cities[i]+"/"+fnames1[j]
            im0 = Image.open(f0)
            im1 = Image.open(f1)
            im0 = smart_resize(im0,width,height)
            im1 = smart_resize(im1,width,height)
            im = Image.new('RGB',(im0.size[0]+im1.size[0],im0.size[1]))
            im.paste(im0,(0,0))
            im.paste(im1,(im0.size[0],0))
            #im.show()
            fname = "_".join(fnames0[j].split("_")[:-1])
            im.save("out/"+fname+".png","PNG")
            sys.stdout.write('.')
    print "\n"

        