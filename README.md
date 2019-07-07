# 2D FeatureScript

## Objective
[School 42](https://www.42.us.org/) RoboLab Hardware piscine: \
learning CAD automation by using FeatureScript produced. FeatureScript is a language create by [Onshape](https://www.onshape.com/). Onshape is a webase CAD designed software, which you can edit and access anywhere when you have a browser.

## 2D project
![2D_Circles](/image/2D_Circles.png)\
Parameter option:
>   * Big Circle Radius
>   * Mid Circle Radius
>   * Lit Circle Radius\
[Source Code](https://github.com/JCTGY/onshape_CAD_FeatureScript/blob/master/2dCircles.fs)\
[Onshape Document](https://cad.onshape.com/documents/7bd52314a96f9a14b24e8ca8/w/290640dd845c1e9488e4008d/e/a40c1ed49153c80a4acac5b6)\
Thanks to @paul_chastell at [onshape forum](https://forum.onshape.com/discussion/11944/question-about-feature-script-sketch-merge-and-constrain-when-using-circular-pattern)\
rotationAround is used to achieve circular pattern in the same sketch
```
        for (var i = 0; i < count; i += 1)
        {
            var rotation = rotationAround(axis, (i / count) * full);
            var cirId = "cir" ~ i;
            var cLit3d = rotation * center;
            var cLit2d = vector(cLit3d[0], cLit3d[1]); 
    
            
            skCircle(sk, cirId, {
                "center" : cLit2d ,
                "radius" : littleCir * millimeter
            });
        }
```
