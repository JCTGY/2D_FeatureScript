# CAD_FeatureScript

* [Objective](#objective)
* [2D project](#2D-project)
* [Lego bricks](#Lego-bricks)

## Objective
school 42 RoboLab learning CAD automation by using FeatureScript produced by [Onshape](https://www.onshape.com/).
Three projects
* 2D sketch circles and arcs with FeatureScript
* Lego bricks: able to change row and colum
* Thread feature

## 2D project
![2D_Circles](/image/2D_Circles.png)\
Parameter option:
>   * Big Circle Radius
>   * Mid Circle Radius
>   * Lit Circle Radius\
[Source Code](https://github.com/JCTGY/onshape_CAD_FeatureScript/blob/master/2D_circles.fs)\
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
![1 X 1 Lego.png](/image/1_X_1_Lego.png)\
