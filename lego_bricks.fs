FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

annotation { "Feature Type Name" : "Lego Bricks" }

export const myFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        // Define the parameters of the feature type
        annotation { "Name" : "Lego Brick Row" }
        isInteger(definition.Row, POSITIVE_COUNT_BOUNDS);
        annotation { "Name" : "Lego Brick Col" }
        isInteger(definition.Col, POSITIVE_COUNT_BOUNDS);      
    }
    {
        // Define the function's action
        var dia = 4.8;
        var width = 8;
        var height = 11.2;
        var thick = 1.6;
        var row = definition.Row;
        var col = definition.Col;
        
        var base_sketch = newSketch(context, id + "base_sketch", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
        });
        // Create the base block
        var block_face = Create_Base_Lego(base_sketch, context, id, row, width, col, height, thick);
        // Create the Top Cir
        var top_cir_face = Create_Top_Circles(block_face, context, id, row, width, col, dia, thick);
        opBoolean(context, id + "boolean1", {
                "tools" : qBodyType(qEverything(EntityType.BODY), BodyType.SOLID),
                "operationType" : BooleanOperationType.UNION
        });
        shell_the_lego(context, id, thick, qNthElement(qCreatedBy(id + "base_extrude", EntityType.FACE), 1));
        if (row > 1 || col > 1)
        {
            if (row > 1 && col > 1){
                create_inner_tube(context, id, row, col, height, width, thick, dia);
            }
            else if (row > 1 || col > 1 && (row != 1 || col != 1)){
                create_solid_tube(context, id, row, col, thick, width, dia, height);
            }
        } 
        create_lego_text(context, id, height, row, col, width, dia, thick, top_cir_face);
        union_all_soild(context, id);
        opDeleteBodies(context, id + "deleteBodies1", {
                "entities" : qSketchFilter(qCreatedBy(id), SketchObject.YES)
        });
       
    });
    
    /*
    **  Create the base square of the lego
    **  default unit is millimeter
    **  Parameters: base_sketch: [Sketch]sketch for the base of the lego
    **              row: [number]row of the lego = definition.Row
    **              col: [number]colum of the lego = definition.Col
    **              height: [number]height of the lego = 11.2
    **              thick: [number] thick size of the lego = 1.6
    **             
    **  Return: [Query]face of the top birck-> for creating the stub later
    */
    function Create_Base_Lego(base_sketch is Sketch, context is Context, id is Id, row is number, width is number, col is number, height is number, thick is number)
    {
        skRectangle(base_sketch, "brick_base", {
                "firstCorner" : vector(0, 0) * millimeter,
                "secondCorner" : vector(width * row, width * col) * millimeter
        });
        skSolve(base_sketch);
        opExtrude(context, id + "base_extrude", {
                "entities" : qSketchRegion(id + "base_sketch"),
                "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + "base_sketch")}).normal,
                "endBound" : BoundingType.BLIND,
                "endDepth" : (height - thick) * millimeter
        });
        return qNthElement(qCreatedBy(id + "base_extrude", EntityType.FACE), 2);
    }
    
    /*
    **  Create the top stub
    **  default unit is millimeter
    **  Parameters: base_sketch: [Sketch]sketch for the base of the lego
    **              row: [number]row of the lego = definition.Row
    **              col: [number]colum of the lego = definition.Col
    **              dia: [number]radius of the stub = 4.8
    **              thick: [number] thick size of the lego = 1.6
    **             
    **  Return: [Query]face of the top stub-> for adding text later
    */
    function Create_Top_Circles(block_face is Query, context is Context, id is Id, row is number, width is number, col is number, dia is number, thick is number)
    {
        var sketch_top_circle = newSketch(context, id + "sk_top_cir", {
            "sketchPlane" : block_face
        });
        var cir_id = 0;
        for (var c = 0; c < col; c += 1)
        {
            for (var r = 0; r < row; r += 1)
            {
                skCircle(sketch_top_circle, "top_cir" ~ cir_id, {
                        "center" : vector((width / 2) + width * r, (width / 2) + width * c) * millimeter,
                        "radius" : dia / 2 * millimeter
                });
                cir_id += 1;
            }
        }
        skSolve(sketch_top_circle);
        opExtrude(context, id + "top_cir_extrude", {
                "entities" : qSketchRegion(id + "sk_top_cir"),
                "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + "sk_top_cir")}).normal,
                "endBound" : BoundingType.BLIND,
                "endDepth" : thick * millimeter
        });
        return qNthElement(qCreatedBy(id + "top_cir_extrude", EntityType.FACE), 1);
    }
    
    /*
    **  function for create tube support brick up than the 2*2
    **  Hollow tube at the bottom of the lego
    **  default unit is millimeter
    **  Parameters: base_sketch: [Sketch]sketch for the base of the lego
    **              row: [number]row of the lego = definition.Row
    **              col: [number]colum of the lego = definition.Col
    **              height: [number]height of the lego = 11.2
    **              width: [number]length of the lego side = 8
    **              dia: [number]radius of the stub = 4.8
    **              thick: [number] thick size of the lego = 1.6
    **             
    **  Return: NULL
    */
    function create_inner_tube(context is Context, id is Id, row is number, col is number, height is number, width is number, thick is number, dia is number)
    {
            var tube_sketch = newSketch(context, id + "tube_sketch", {
                    "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
            var tube_cir_id = 0;

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
            var tube_in_sketch = newSketch(context, id + "tube_in_sketch", {
                    "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
            var tube_in_cir_id = 0;

            for (var c = 2; c <= col ; c += 1)
            {
                for (var r = 2; r <= row ; r += 1)
                {
                    skCircle(tube_in_sketch, "tube_cir" ~ tube_in_cir_id, {
                            "center" : vector(width * (r - 1), width * (c - 1)) * millimeter,
                            "radius" : dia / 2 * millimeter
                    });
                    tube_in_cir_id += 1;
                }
            }
            skSolve(tube_in_sketch);
            opExtrude(context, id + "tube_in_cir_extrude", {
                "entities" : qSketchRegion(id + "tube_in_sketch"),
                "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + "tube_in_sketch")}).normal,
                "endBound" : BoundingType.BLIND,
                "endDepth" : (height - thick) * millimeter
            });
            // Use the big hole extrusion to substract it from the part
            opBoolean(context, id + "tube_cir", {
            "tools" : qCreatedBy(id + "tube_in_cir_extrude", EntityType.BODY),
            "targets" : qCreatedBy(id + "tube_cir_extrude", EntityType.BODY),
            "operationType" : BooleanOperationType.SUBTRACTION
            });
    }
        
    /*
    **  Fuction for tube that is X * 1 || 1 * X bricks
    **  Solid tube at the bottom of the lego
    **  default unit is millimeter
    **  Parameters: base_sketch: [Sketch]sketch for the base of the lego
    **              row: [number]row of the lego = definition.Row
    **              col: [number]colum of the lego = definition.Col
    **              height: [number]height of the lego = 11.2
    **              width: [number]length of the lego side = 8
    **              dia: [number]radius of the stub = 4.8
    **              thick: [number] thick size of the lego = 1.6
    **             
    **  Return: NULL
    */
    function create_solid_tube(context is Context, id is Id, row is number, col is number, thick is number, width is number, dia is number, height is number)
    {
        var stube_sketch = newSketch(context, id + "stube_sketch", {
            "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
        var stube = (row == 1) ? col : row;
        var stube_cir_id = 0;

        for (var count = 1; count < stube ; count += 1)
        {
            var c = (row == 1) ? count - 1 : 0;
            var r = (row == 1) ? 0 : count - 1;
            var counter_c = (row == 1) ? 1 : 2;
            var counter_r = (row == 1) ? 2 : 1;
            skCircle(stube_sketch, "tube_cir" ~ stube_cir_id, {
                    "center" : vector(width / counter_r + width * r, width / counter_c + width * c) * millimeter,
                    "radius" : (dia - thick)  / 2 * millimeter
            });
            stube_cir_id += 1;
        }
        skSolve(stube_sketch);
        
        opExtrude(context, id + "stube_cir_extrude", {
            "entities" : qSketchRegion(id + "stube_sketch"),
            "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + "stube_sketch")}).normal,
            "endBound" : BoundingType.BLIND,
            "endDepth" : (height - thick) * millimeter
        });        
    }
    
    /*
    **  Union all bodies
    **  default unit is millimeter
    **  Parameters: context: [Contex]context of the document
    **              id: [ID] id of the document
    **                         
    **  Return: NULL
    */
    function union_all_soild(context is Context, id is Id)
    {
        opBoolean(context, id + "union_all", {
                "tools" : qAllNonMeshSolidBodies(),
                "operationType" : BooleanOperationType.UNION
        });
    }
    
    /*
    **  shell the lego of stub and the base
    **  default unit is millimeter
    **  Parameters: context: [Contex]context of the document
    **              id: [ID] id of the document
    **              thick: [number] thick size of the lego = 1.6
    **              shell_face: [Query] face of the bottom base
    **                         
    **  Return: NULL
    */
    function shell_the_lego(context is Context, id is Id, thick is number, shell_face is Query)
    {
        opShell(context, id + "shell_base", {
                "entities" : shell_face,
                "thickness" : -thick * millimeter
        });
    }
    
    /*
    **  add lego tex on the top
    **  default unit is millimeter
    **  Parameters: context: [Contex]context of the document
    **              id: [ID] id of the document
    **              height: [number]height of the lego = 11.2
    **              row: [number]row of the lego = definition.Row
    **              col: [number]colum of the lego = definition.Col
    **              width: [number]length of the lego side = 8
    **              dia: [number]radius of the stub = 4.8
    **              thick: [number] thick size of the lego = 1.6
    **              top_cir_face: [Query] the top face of the stub
    **                         
    **  Return: NULL
    */
    function create_lego_text(context is Context, id is Id, height is number, row is number, col is number, width is number, dia is number, thick is number, top_cir_face is Query){
        var sk_text = newSketch(context, id + "sk_text", {
                "sketchPlane" : top_cir_face
        });
        var text_id = 0;
        for (var c = 1; c <= col; c += 1)
        {
            for (var r = 1; r <= row; r += 1)
            {
                skText(sk_text, "text1" ~text_id, {
                    "text" : "LEGO",
                    "fontName" : "OpenSans-Regular.ttf",
                    "firstCorner" : vector((width / 2 * sqrt(2) - dia *0.80) + width * (r - 1), (width / 2 * sqrt(2) - dia *0.50) + width * (c - 1)) * millimeter,
                    "secondCorner" : vector((width / 2 * sqrt(2) - dia *0.80) + dia * 0.10 + width * (r - 1), (width / 2 * sqrt(2) - dia *0.50) + dia * 0.25 + width * (c - 1)) * millimeter
                });
                text_id += 1;
            }
        }
        skSolve(sk_text);
        opExtrude(context, id + "extrude_text", {
                "entities" : qSketchRegion(id + "sk_text", true),
                "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + "sk_text")}).normal,
                "endBound" : BoundingType.BLIND,
                "endDepth" : thick  / 8 * millimeter
        });

    }
    
        

