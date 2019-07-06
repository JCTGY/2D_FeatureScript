# CAD_FeatureScript

## Objective
school 42 RoboLab learning CAD automation by using FeatureScript produced by [Onshape](https://www.onshape.com/).
Three projects
* 2D sketch circles and arcs with FeatureScript
* Lego bricks: able to change row and colum
* Thread feature

* [2D project](#2D-project)
* [Lego bricks](#Lego-bricks)


## 2D project
![2D_Circles](/image/2D_Circles.png)\
Parameter option:
>   * Big Circle Radius
>   * Mid Circle Radius
>   * Lit Circle Radius\
[Source Code](https://github.com/JCTGY/onshape_CAD_FeatureScript/blob/master/2D_circles.fs)\
[Onshape Document](https://cad.onshape.com/documents/7bd52314a96f9a14b24e8ca8/w/290640dd845c1e9488e4008d/e/a40c1ed49153c80a4acac5b6)\
Thanks to @paul_chastell at [onshape forum](https://forum.onshape.com/discussion/11944/question-about-feature-script-sketch-merge-and-constrain-when-using-circular-pattern)\
rotationAround is used to achieve circular pattern in the same sketch
```
        for (var i = 0; i < count; i += 1)
        {
            var rotation = rotationAround(axis, (i / count) * full);
            var cirId = "cir" ~ i;
            var c_lit3d = rotation * center;
            var c_lit2d = vector(c_lit3d[0], c_lit3d[1]); 
    
            
            skCircle(sk, cirId, {
                "center" : c_lit2d ,
                "radius" : little_cir * millimeter
            });
        }
```

## Lego bricks
Parameter option:
>   * Row Lego\
>   * Col Lego\
[Source Code](https://github.com/JCTGY/onshape_CAD_FeatureScript/blob/master/lego_bricks.fs)\
[Onshape Document](https://cad.onshape.com/documents/da6b009e9c013270aeae4cd8/w/05c0f5a10696f0c50747bc21/e/385ac05fe04a705f8d000c23)


![1 X 1 Lego.png](/image/1X1_Lego.png)\
In created bottom hollow tube for lego: \
add true add the back, which means only extrude the part outter ring
```
opExtrude(context, id + "tube_cir_extrude, true",
```
Insted of the bottom code. Will still need to delet the inner tube body to make the hollow tube.\
However, will leave the code as it is for future references
```
            for (var c = 2; c <= col ; c += 1)
            {
                for (var r = 2; r <= row ; r += 1)
                {
                    skCircle(tube_sketch, "tube_cir" ~ tube_cir_id, {
                            "center" : vector(width * (r - 1), width * (c - 1)) * millimeter,
                            "radius" : dia / 4.8 * 6.4  / 2 * millimeter
                    });
                    tube_cir_id += 1;
                }
            }
            skSolve(tube_sketch);
            opExtrude(context, id + "tube_cir_extrude", {
                "entities" : qSketchRegion(id + "tube_sketch"),
                "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + "tube_sketch")}).normal,
                "endBound" : BoundingType.BLIND,
                "endDepth" : (height - thick) * millimeter
            });
```

![2 X 2 Lego.png](/image/2X2_Lego.png)
![2 X 2 Lego.png](/image/2X2_Lego_Back.png)\
X * 1 || 1 * X lego brick have a solid supporting tube\
![5 X 1 Lego.png](/image/5X1_Lego.png)
![5 X 1 Lego.png](/image/5X1_Lego_Back.png)
![10 X 10 Lego.png](/image/10X10_Lego.png)
