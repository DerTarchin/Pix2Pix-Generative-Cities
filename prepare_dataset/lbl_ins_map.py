import cv2 as cv
import os
import numpy as np
import thread
import time
import sys

pcount = [0]

hist = []

T = time.time()

def distance(c1,c2):
    return ((c1[0]-c2[0])**2 + (c1[1]-c2[1])**2 + (c1[2]-c2[2])**2)**0.5


def process(impath):
    global hist, T
    cmap = [(  0,  0,  0), (  0,  0,  0), (  0,  0,  0), (  0,  0,  0), (  0,  0,  0), (111, 74,  0), ( 81,  0, 81),
            (128, 64,128), (244, 35,232), (250,170,160), (230,150,140), ( 70, 70, 70), (102,102,156), (190,153,153),
            (180,165,180), (150,100,100), (150,120, 90), (153,153,153), (153,153,153), (250,170, 30), (220,220,  0),
            (107,142, 35), (152,251,152), ( 70,130,180), (220, 20, 60), (255,  0,  0), (  0,  0,142), (  0,  0, 70),
            (  0, 60,100), (  0,  0, 90), (  0,  0,110), (  0, 80,100), (  0,  0,230), (119, 11, 32), (  0,  0,142)]

    im = cv.imread(impath)
    print impath
    
    h,w,_ = im.shape

    imo = np.zeros((h,w),np.uint8)
    imi = np.zeros((h,w),np.uint8)
    imo.fill(2)

    bord = 10

    def save():
        g = ".".join(impath.split(".")[:-1])
        g1,g2 = "/".join(g.split("/")[:-1]), g.split("/")[-1]
        cv.imwrite(g1+"/out/"+g2+"_lbl"+".png",imo)
        cv.imwrite(g1+"/out/"+g2+"_ins"+".png",imi)


    def label0(c):
        mindist = float("inf")
        minind = 0
        
        for k in range(len(cmap)):
            d = distance(c,cmap[k])
            if d < 30:
                minind = k
                mindist = d
                break
            if d <= mindist:
                minind = k
                mindist = d

        hist.append((c,minind,1))
        return minind

    def label(c):

        for i in range(len(hist)):
            if hist[i][0] == c:
                hist[i] = (hist[i][0], hist[i][1], hist[i][2]+1)
                return hist[i][1]
        return label0(c)
        

    for i in range(bord,h-bord):

        for j in range(bord,w-bord):
            c = im[i,j]
            c = (c[2],c[1],c[0])
            
            minind = label(c)

            imo[i,j] = minind

            if minind == 26:
                imi[i,j] = 124
            elif minind == 25:
                imi[i,j] = 120
            elif minind == 24:
                imi[i,j] = 116
            elif minind == 28:
                imi[i,j] = 131
            elif minind == 33:
                imi[i,j] = 150
            elif minind == 32:
                imi[i,j] = 146
            elif minind == 27:
                imi[i,j] = 139

        if i % 10 == 0:  
            hist = sorted(hist, key=lambda x: -x[2])[:100]

            print "row",i,h
            #print len(hist)
        if i % 100 == 0:
            save()
    print "done", time.time()-T
    T = time.time()
    
    save()
    pcount[0] -= 1

if __name__ == "__main__":
    a1 = int(sys.argv[1])
    a2 = int(sys.argv[2])

    d = "input_dir"
    images = os.listdir(d)
    images.sort()

    R = []
    batch = 5
    for i in range(a1,a2,batch):
        R.append([i,i+batch])

    for r in R:
        print r
        pcount = [0]
        for f in images[r[0]:r[1]]:
            if not f.startswith("."):
                process(d+"/"+f)
                #thread.start_new_thread (process, (d+"/"+f,))
                pcount[0] += 1

        while pcount[0] != 0:
            time.sleep(3.0)
            print pcount, "left"


