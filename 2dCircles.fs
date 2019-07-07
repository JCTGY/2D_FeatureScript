FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

/*
**  LENTHBIGCIR: radius of big circle
**      default : 60 millimeter
**  LENTHMIDCIR: radius of middle circle
**      default : 60 millimeter
**  LENTHLITCIR: radius of little circle
**      default : 60 millimeter
*/
export const LENTHBIGCIR = {
    (millimeter) : [0, 60, 80],
} as LengthBoundSpec;
export const LENTHMIDCIR = {
    (millimeter) : [0, 30, 50],
} as LengthBoundSpec;
export const LENTHLITCIR = {
    (millimeter) : [0, 10, 15],
} as LengthBoundSpec;
annotation { "Feature Type Name" : "Circles" }
export const circles = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        /* 
        ** Define the parameters of the feature type
        */
        annotation { "Name" : "Big circle radius" }
        isLength(definition.bigCir, LENTHBIGCIR);
        annotation { "Name" : "Mid circle radius" }
        isLength(definition.midCir, LENTHMIDCIR);
        annotation { "Name" : "Lit circle radius" }
        isLength(definition.litCir, LENTHLITCIR);
        
    }
    {
        // Define the function's action
        var littleCir is number = definition.litCir / millimeter;
        var middleCir is number = definition.midCir / millimeter;
        var bigCir is number = definition.bigCir / millimeter;
        var axis is Line = line(vector(0, 0, 0) * meter, vector(0, 0, 1));
        var full = PI * 2 * radian;
        var count is number = 6;
        
        /* 
        **  Set the sketch plane
        **  Create the center circles (mid and big)
        */
        var sk is Sketch = newSketch(context, id + "skCenterCir", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
        });
        skCircle(sk, "bigMidCir", {
                "center" : vector(0, 0) * millimeter,
                "radius" : bigCir * millimeter
        });
        skCircle(sk, "midMidCir", {
                "center" : vector(0, 0) * millimeter,
                "radius" : middleCir * millimeter
        });
        /*
        **  draw little circles and arcs at each rotation for loops
        **  count: number of objects want to draw at each rotation
        */
        drawLitCir(context, id, sk, littleCir, middleCir, count, axis, full);
        count = 4;
        drawCornerArc(context, id, sk, bigCir, count, axis, full);
        drawTopArc(context, id, sk, bigCir, count, axis, full);
        drawOneCorner(context, id, sk, bigCir, count, axis, full);
        skSolve(sk);

    });
    
    /* 
    ** Draw the center 6 little circles
    ** Parameters: sk is the newsketch, littleCir is the radius of the little circle,
    **             middleCir is the radius of the middle circles, count is the number of little circles,
    **             axis is the rotation line, and full is the radian of 360 
    ** Return: NULL
    */
    function drawLitCir(context is Context, id is Id, sk is Sketch, littleCir is number, middleCir is number, count is number, axis is Line, full){
        
        var center is Vector = vector(0, middleCir, 0) * millimeter;
        // Loop through the required number of instances, transform the points and create the geometry
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
        
    }
    
    /* 
    ** Draw the 4 biggest corner arcs
    ** Parameters: sk is the newsketch, bigCir is the radius of the big circle,
    **             count is the number of arcs, axis is the rotation line, 
    **             and full is the radian of 360 
    ** Return: NULL
    */
    function drawCornerArc(context is Context, id is Id, sk is Sketch, bigCir is number, count is number, axis is Line, full){
        
        var start is Vector = vector(bigCir, 0, 0) * millimeter;
        var mid is Vector = vector(bigCir, bigCir * 2, 0) * millimeter;
        var end is Vector = vector(0, bigCir, 0) * millimeter;
        // Loop through the required number of instances, transform the points and create the geometry
        for (var i = 0; i < count; i += 1)
        {
            var rotation = rotationAround(axis, (i / count) * full);
            var arcId = "arc1" ~ i;
            var start3d = rotation * start;
            var mid3d = rotation * mid;
            var end3d = rotation * end;
            
            var start2d = vector(start3d[0], start3d[1]);
            var mid2d = vector(mid3d[0], mid3d[1]);
            var end2d = vector(end3d[0], end3d[1]);
            skArc(sk, arcId, {
                "start" : start2d,
                "mid" : mid2d,
                "end" : end2d
            }); 
        }
    }
    
    /* 
    ** Draw the top 4 middle arcs
    ** Parameters: sk is the newsketch, bigCir is the radius of the big circle,
    **             count is the number of arcs, axis is the rotation line, 
    **             and full is the radian of 360 
    ** Return: NULL
    */
    function drawTopArc(context is Context, id is Id, sk is Sketch, bigCir is number, count is number, axis is Line, full){
        
        var start is Vector = vector(bigCir, bigCir * 2, 0) * millimeter;
        var mid is Vector = vector(0, bigCir, 0) * millimeter;
        var end is Vector = vector(-bigCir, bigCir * 2, 0) * millimeter;
        // Loop through the required number of instances, transform the points and create the geometry
        for (var i = 0; i < count; i += 1)
        {
            var rotation = rotationAround(axis, (i / count) * full);
            var arcId = "arc2" ~ i;
            var start3d = rotation * start;
            var mid3d = rotation * mid;
            var end3d = rotation * end;
            
            var start2d = vector(start3d[0], start3d[1]);
            var mid2d = vector(mid3d[0], mid3d[1]);
            var end2d = vector(end3d[0], end3d[1]);
            skArc(sk, arcId, {
                "start" : start2d,
                "mid" : mid2d,
                "end" : end2d
            });
        }
    }
    
    /* 
    ** Draw the 4 smallest arcs at the corner
    ** Parameters: sk is the newsketch, bigCiris the radius of the big circle,
    **             count is the number of arcs, axis is the rotation line, 
    **             and full is the radian of 360 
    ** Return: NULL
    */
    function drawOneCorner(context is Context, id is Id, sk is Sketch, bigCir is number, count is number, axis is Line, full){
        
        var start is Vector = vector(bigCir, bigCir * 2, 0) * millimeter;
        var mid is Vector = vector(bigCir * 2 - (bigCir / sqrt(2)), bigCir * 2 - (bigCir / sqrt(2)), 0) * millimeter;
        var end is Vector = vector(bigCir * 2, bigCir, 0) * millimeter;
        // Loop through the required number of instances, transform the points and create the geometry
        for (var i = 0; i < count; i += 1)
        {
            var rotation = rotationAround(axis, (i / count) * full);
            var arcId = "arc3" ~ i;
            var start3d = rotation * start;
            var mid3d = rotation * mid;
            var end3d = rotation * end;
            
            var start2d = vector(start3d[0], start3d[1]);
            var mid2d = vector(mid3d[0], mid3d[1]);
            var end2d = vector(end3d[0], end3d[1]);
            skArc(sk, arcId, {
                "start" : start2d,
                "mid" : mid2d,
                "end" : end2d
            }); 
        }
    }
    
        
        

        

