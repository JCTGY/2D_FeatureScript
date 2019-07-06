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
        isLength(definition.big_cir, LENTHBIGCIR);
        annotation { "Name" : "Mid circle radius" }
        isLength(definition.mid_cir, LENTHMIDCIR);
        annotation { "Name" : "Lit circle radius" }
        isLength(definition.lit_cir, LENTHLITCIR);
        
    }
    {
        // Define the function's action
        var little_cir is number = definition.lit_cir / millimeter;
        var middle_cir is number = definition.mid_cir / millimeter;
        var big_cir is number = definition.big_cir / millimeter;
        var axis is Line = line(vector(0, 0, 0) * meter, vector(0, 0, 1));
        var full = PI * 2 * radian;
        var count is number = 6;
        
        /* 
        **  Set the sketch plane
        **  Create the center circles (mid and big)
        */
        var sk is Sketch = newSketch(context, id + "sk_center_cir", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
        });
        skCircle(sk, "big_mid_cir", {
                "center" : vector(0, 0) * millimeter,
                "radius" : big_cir * millimeter
        });
        skCircle(sk, "mid_mid_cir", {
                "center" : vector(0, 0) * millimeter,
                "radius" : middle_cir * millimeter
        });
        /*
        **  draw little circles and arcs at each rotation for loops
        **  count: number of objects want to draw at each rotation
        */
        draw_lit_cir(context, id, sk, little_cir, middle_cir, count, axis, full);
        count = 4;
        draw_corner_arc(context, id, sk, big_cir, count, axis, full);
        draw_top_arc(context, id, sk, big_cir, count, axis, full);
        draw_one_corner(context, id, sk, big_cir, count, axis, full);
        skSolve(sk);

    });
    
    /* 
    ** Draw the center 6 little circles
    ** Parameters: sk is the newsketch, little_cir is the radius of the little circle,
    **             middle cir is the radius of the middle circles, count is the number of little circles,
    **             axis is the rotation line, and full is the radian of 360 
    ** Return: NULL
    */
    function draw_lit_cir(context is Context, id is Id, sk is Sketch, little_cir is number, middle_cir is number, count is number, axis is Line, full){
        
        var center is Vector = vector(0, middle_cir, 0) * millimeter;
        // Loop through the required number of instances, transform the points and create the geometry
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
        
    }
    
    /* 
    ** Draw the 4 biggest corner arcs
    ** Parameters: sk is the newsketch, big_cir is the radius of the big circle,
    **             count is the number of arcs, axis is the rotation line, 
    **             and full is the radian of 360 
    ** Return: NULL
    */
    function draw_corner_arc(context is Context, id is Id, sk is Sketch, big_cir is number, count is number, axis is Line, full){
        
        var start is Vector = vector(big_cir, 0, 0) * millimeter;
        var mid is Vector = vector(big_cir, big_cir * 2, 0) * millimeter;
        var end is Vector = vector(0, big_cir, 0) * millimeter;
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
    ** Parameters: sk is the newsketch, big_cir is the radius of the big circle,
    **             count is the number of arcs, axis is the rotation line, 
    **             and full is the radian of 360 
    ** Return: NULL
    */
    function draw_top_arc(context is Context, id is Id, sk is Sketch, big_cir is number, count is number, axis is Line, full){
        
        var start is Vector = vector(big_cir, big_cir * 2, 0) * millimeter;
        var mid is Vector = vector(0, big_cir, 0) * millimeter;
        var end is Vector = vector(-big_cir, big_cir * 2, 0) * millimeter;
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
    ** Parameters: sk is the newsketch, big_cir is the radius of the big circle,
    **             count is the number of arcs, axis is the rotation line, 
    **             and full is the radian of 360 
    ** Return: NULL
    */
    function draw_one_corner(context is Context, id is Id, sk is Sketch, big_cir is number, count is number, axis is Line, full){
        
        var start is Vector = vector(big_cir, big_cir * 2, 0) * millimeter;
        var mid is Vector = vector(big_cir * 2 - (big_cir / sqrt(2)), big_cir * 2 - (big_cir / sqrt(2)), 0) * millimeter;
        var end is Vector = vector(big_cir * 2, big_cir, 0) * millimeter;
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
