FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

/*
**  enum for diameter of thread
**  source form https://www.fullerfasteners.com/tech/basic-metric-thread-chart-m1-m100-2/
*/
export enum Thread_Size
{
    annotation { "Name" : "Diameter M1" } M1,
    annotation { "Name" : "Diameter M2" } M2,
    annotation { "Name" : "Diameter M3" } M3,
    annotation { "Name" : "Diameter M4" } M4,
    annotation { "Name" : "Diameter M5" } M5,
    annotation { "Name" : "Diameter M6" } M6,
    annotation { "Name" : "Diameter M7" } M7,
    annotation { "Name" : "Diameter M8" } M8,
    annotation { "Name" : "Diameter M9" } M9,
    annotation { "Name" : "Diameter M10" } M10
    
}

export const LENGTH_THREAD =
{
    (millimeter) : [0, 10, 20]
} as LengthBoundSpec;

annotation { "Feature Type Name" : "Thread V2" }
export const myFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        /* 
        **  Define the parameters of the feature type
        **  definition.hole: selection of a cylinder face
        **  definition.size size form Thread_Size enum
        */
        annotation { "Name" : "Hole", "Filter" : EntityType.FACE && GeometryType.CYLINDER, "MaxNumberOfPicks" : 1 }
        definition.hole is Query;
        annotation { "Name" : "Basic Metric Thread" }
        definition.size is Thread_Size;
        annotation { "Name" : "Length of Thread" }
        isLength(definition.Length, LENGTH_THREAD);  
    }
    {
        /*
        **  thread_face: [Query]face of the hole
        **  othreadAxis: [Line]line of the hole axis
        **  xDirection: [Vector]the x-axis direction according to the hole
        **  zDirection: [Vector]the z-axis direction according to the hole 
        **  pitch: [ValueWithUnits]pitch size of the thread as millimeter
        **  cut_size: [number]length of the triangle side  
        **
        **  Objective:
        **  42 school's RoboLab project. Use onshape feature scripts to create thread tool
        **  thread size M1 ~ M10 source form: FULLER Basic Metric Thread Chart (M1 – M100)
        **                          https://www.fullerfasteners.com/tech/basic-metric-thread-chart-m1-m100-2/
        **
        **  Method:
        **  Create cylinde in the hole -> helix for cut path -> curve_point plane to sketch triangle cutter
        **  -> sweep with remove to create the thread -> clean up the unsued objects
        **
        **  Tips: can modifides pitch size and cut_sized to the desired size of the thread uint in millimeter
        */
        var thread_face = definition.hole; 
        var othreadAxis is Line = evAxis(context, { "axis" : thread_face });
        var threadRad is ValueWithUnits = evSurfaceDefinition(context, { "face" : thread_face }).radius * meter / millimeter;
        var xDirection is Vector = othreadAxis.direction;
        var zDirection is Vector = perpendicularVector(othreadAxis.direction);
        var cSys is CoordSystem = coordSystem(othreadAxis.origin, xDirection, zDirection);
        var pitch is ValueWithUnits = pitch_size(context, definition);

        create_cylinder(context, id, definition, othreadAxis, threadRad);
        create_helix_cutter(context, id, definition, xDirection, zDirection, othreadAxis, threadRad, pitch);
        var cut_plane is array = [];
        var cutter_size = getVariable(context, "cutter_size");
        cut_plane = append(cut_plane, qCreatedBy(id + "helix1", EntityType.EDGE));
        cut_plane = append(cut_plane, qNthElement(qCreatedBy(id + "helix1", EntityType.VERTEX), 0));
        create_curve_point_plane(context, id, cut_plane);
        create_triangle_cutter(context, id, cut_plane, cutter_size, xDirection);
        sweep_with_cutter(context, id);
        clean_up_unuse(context, id);
    });
    
    /*
    **  get the proper pitch size and set cutter_size value from definetion.size
    **  Parameter: context: file's context, 
    **             definition: include definition.hole and definition.size
    **  return: [ValueWithUnits] pitch size in millimeter
    */
    function pitch_size(context is Context, definition is map)
    {
        var pitch is ValueWithUnits = 0 * millimeter;
        var cutter_size = 1;
        if (definition.size == Thread_Size.M1){
            pitch = 0.25 * millimeter;
            cutter_size = 0.2;
        }
        else if (definition.size == Thread_Size.M2){
            pitch = 0.4 * millimeter;
            cutter_size = 0.25;
        }
        else if (definition.size == Thread_Size.M3){
            pitch = 0.5 * millimeter;
            cutter_size = 0.35;
        }
        else if (definition.size == Thread_Size.M4){
            pitch = 0.6 * millimeter;
            cutter_size = 0.4;
        }
        else if (definition.size == Thread_Size.M5){
            pitch = 0.8 * millimeter;
            cutter_size = 0.5;
        }
        else if (definition.size == Thread_Size.M6){
            pitch = 1 * millimeter;
            cutter_size = 0.75;
        }
        else if (definition.size == Thread_Size.M7){
            pitch = 1 * millimeter;
            cutter_size = 0.75;
        }
        else if (definition.size == Thread_Size.M8){
            pitch = 1.25 * millimeter;
            cutter_size = 1;
        }  
        else if (definition.size == Thread_Size.M9){
            pitch = 1.25 * millimeter;
            cutter_size = 1;
        }
        else if (definition.size == Thread_Size.M10){
            pitch = 1.5 * millimeter;
            cutter_size = 1.25;
        }
        setVariable(context, "cutter_size", cutter_size);
        return pitch;
    }
    
    /*
    **  Create the cylinder of the center of the hole for helix
    **  Parameter: context: file's context, 
    **             definition: include definition.hole and definition.size
    **             othereadAxis: [Line]the origin axis of the hole
    **             threadRad: [ValueWithUnits]hole radius
    **  return: NULL
    */
    function create_cylinder(context is Context, id is Id, definition is map, othreadAxis is Line, threadRad is ValueWithUnits)
    {
        var sketch_plane = qNthElement(qEntityFilter(qAdjacent(definition.hole, AdjacencyType.EDGE), EntityType.FACE), 0);
        var sk_edgecir = newSketch(context, id + "sketch1", {
                "sketchPlane" : sketch_plane
        });
        var plane_circle is Plane = evPlane(context, {
                "face" : sketch_plane
        });
        var d2_center = worldToPlane(plane_circle, othreadAxis.origin * meter / millimeter);
        skCircle(sk_edgecir, "circle1", {
                "center" : vector(d2_center[0] , d2_center[1]) / meter * millimeter,
                "radius" :  threadRad / meter * millimeter
        });
        skSolve(sk_edgecir);
        opExtrude(context, id + "extrude1", {
                "entities" : qNthElement(qCreatedBy(id + "sketch1", EntityType.EDGE), 0),
                "direction" : plane_circle.normal * -1,
                "endBound" : BoundingType.BLIND,
                "endDepth" : definition.Length
        });
        
    }
    /*
    **  Create center helix for the cutter path
    **  Parameter: context: file's context, 
    **             definition: include definition.hole and definition.size
    **             xDirection: [Vector]the x-axis direction according to the hole
    **             zDirection: [Vector]the z-axis direction according to the hole
    **             othereadAxis: [Line]the origin axis of the hole
    **             threadRad: [ValueWithUnits]hole radius
    **             pitch: [ValueWithUnits]pitch size of the thread
    **  return: NULL
    */
    function create_helix_cutter(context is Context, id is Id, definition is map, xDirection is Vector, zDirection is Vector, othreadAxis is Line, threadRad is ValueWithUnits, pitch is ValueWithUnits)
    {
        var axistart = othreadAxis.origin * millimeter / meter;
        var origin = axistart /millimeter;
        var ogvector is Vector = vector(0, 0, 0);
        if (xDirection[0] != 0){
            ogvector = vector(0, origin[1], origin[2]);
        }
        else if (xDirection[1] != 0){
            ogvector = vector(origin[0], 0, origin[2]);
        }
        else if (xDirection[2] != 0){
            ogvector = vector(origin[0], origin[1], 0);
        }
        opHelix(context, id + "helix1", {
            "direction" : xDirection,
            "axisStart" : axistart /millimeter *meter,
            "startPoint" : ogvector * meter + zDirection * (threadRad / meter * millimeter),
            "interval" : [0, definition.Length / pitch],
            "clockwise" : true,
            "helicalPitch" : pitch,
            "spiralPitch" : 0 * millimeter
        });
    }
    
    /*
    **  Create create_curve_point_plane
    **  Parameter: context: file's context, 
    **             cut_plane: array [0]: [Query]helix EntityType.EDGE, 
    **                              [1]: [Query]helix start position EntityType.VERTEX
    **  return: NULL
    */
    function create_curve_point_plane(context is Context, id is Id, cut_plane is array)
    {  
        cPlane(context, id + "cplane", { 
                "entities" : qUnion(cut_plane),
                "cplaneType" : CPlaneType.CURVE_POINT, 
                "flipAlignment" : false, 
                "width" : 6.0 * inch, 
                "height" : 6.0 * inch 
        });      
    }  
     
     
    /*
    **  Crete the triangle cutter
    **  Parameter: context: file's context, 
    **             cut_plane: array [0]: [Query]helix EntityType.EDGE, 
    **                              [1]: [Query]helix start position EntityType.VERTEX
    **             cutter_size: [number]length of the triangle's side
    **             xDirection: [Vector]the x-axis direction according to the hole
    **  return: NULL
    */
    function create_triangle_cutter(context is Context, id is Id, cut_plane is array, cutter_size is number, xDirection is Vector)
    {
        var cplane = evPlane(context, {
                "face" : qCreatedBy(id + "cplane", EntityType.FACE)
        });
        var sketch_cut = newSketchOnPlane(context, id + "sketch2", {
                "sketchPlane" : cplane
        });
        var point1 = evVertexPoint(context, {
                "vertex" : cut_plane[1]
        });
        var p1 is Vector = vector(0, 0);
        var p2 is Vector = vector(0, 0);
        var p3 is Vector = vector(0, 0);
        var point_st = worldToPlane(cplane, point1);
        if (xDirection[0] != 0){
            p1 = point_st / meter * millimeter;
            p2 = point_st / meter * millimeter - vector(cutter_size, 0) * millimeter;
            p3 =  point_st / meter * millimeter + vector(dot(vector(0, cutter_size) * millimeter, vector(0, -sqrt(3) / 4)), dot(vector(0, cutter_size) * millimeter, vector(0, 3/4)));
        }
        else if (xDirection[1] != 0){
            p1 = point_st / meter * millimeter;
            p2 = point_st / meter * millimeter + vector(cutter_size, 0) * millimeter;
            p3 =  point_st / meter * millimeter - vector(dot(vector(0, cutter_size) * millimeter, vector(0, -sqrt(3) / 4)), dot(vector(0, cutter_size) * millimeter, vector(0, 3/4)));
        }
        else if (xDirection[2] != 0){
            p1 = point_st / meter * millimeter;
            p2 = point_st / meter * millimeter - vector(0, cutter_size) * millimeter;
            p3 =  point_st / meter * millimeter - vector(dot(vector(cutter_size, 0) * millimeter, vector(3/4, 0)), dot(vector(cutter_size, 0) * millimeter, vector(sqrt(3) / 4, 0)));
        }
        skLineSegment(sketch_cut, "line1", {
                "start" : p1,
                "end" : p2
        });
        skLineSegment(sketch_cut, "line2", {
                "start" : p1,
                "end" :  p3
        });
        skLineSegment(sketch_cut, "line3", {
                "start" : p3,
                "end" : p2
        });
        skSolve(sketch_cut);  
    }
    
    /*
    **  sweep_with_cutter with size using cutter triangle as the face
    **  and the helix as the path to follow
    **  Parameter: context: file's context
    **  return: NULL
    */
    function sweep_with_cutter(context is Context, id is Id)
    {
         sweep(context, id + "sweep", {
            "profiles" : qSketchRegion(id + "sketch2"),
            "path" : qCreatedBy(id + "helix1", EntityType.EDGE),
            });
        opBoolean(context, id + "boolean1", {
                "tools" : qCreatedBy(id + "sweep", EntityType.BODY),
                "targets" : qNthElement(qAllNonMeshSolidBodies(), 0),
                "operationType" : BooleanOperationType.SUBTRACTION
        });
        
    }
    
    /*
    **  clean up function
    **  delete sketches and bodies that not use
    **  Parameter: context: file's context
    **  return: NULL
    */
    function clean_up_unuse(context is Context, id is Id)
    {
        opDeleteBodies(context, id + "deleteBodies1", {
                "entities" : qSketchFilter(qCreatedBy(id), SketchObject.YES)
        });
        opDeleteBodies(context, id + "deleteBodies2", {
                "entities" : qCreatedBy(id + "extrude1", EntityType.FACE)
        });
        opDeleteBodies(context, id + "deleteBodies3", {
                "entities" : qCreatedBy(id + "helix1", EntityType.EDGE)
        });
    }

