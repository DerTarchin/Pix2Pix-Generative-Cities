import cv2 as cv
import os
import numpy as np
import math

# lens distortion for google cardboard
def barrel(im,k,s=1):
    h,w,_ = im.shape
    dst = im.copy()
    dst[:,:,:] = 0
    for i in range(h):
        for j in range(w):
            x = j - w/2
            y = i - h/2
            rd = (x**2 + y**2)**0.5
            a = math.atan2(y,x)

            ru = rd*(1+k*(rd**2))*s

            xu = math.cos(a)*ru
            yu = math.sin(a)*ru
            
            iu, ju = int(yu+h/2),int(xu+w/2)
            if 0 < iu < h and 0 < ju < w:
                dst[i,j] = im[iu,ju]
    return dst

if __name__ == "__main__":
    cv.imwrite('out.jpg',barrel(cv.imread("00001-l-outputs.png"),0.00002,s=0.8))

