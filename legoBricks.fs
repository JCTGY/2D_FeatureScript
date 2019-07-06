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
        
        var baseSketch = newSketch(context, id + "baseSketch", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
        });
        // Create the base block
        var blockFace = createBaseLego(baseSketch, context, id, row, width, col, height, thick);
        // Create the Top Cir
        var topCirFace is Query = createTopCircles(blockFace, context, id, row, width, col, dia, thick);
        opBoolean(context, id + "boolean1", {
                "tools" : qBodyType(qEverything(EntityType.BODY), BodyType.SOLID),
                "operationType" : BooleanOperationType.UNION
        });
        shellTheLego(context, id, thick, qNthElement(qCreatedBy(id + "baseExtrude", EntityType.FACE), 1));
        if (row > 1 || col > 1)
        {
            if (row > 1 && col > 1){
                createIinnerTube(context, id, row, col, height, width, thick, dia);
            }
            else if (row > 1 || col > 1 && (row != 1 || col != 1)){
                createSolidTube(context, id, row, col, thick, width, dia, height);
            }
        } 
        createLegoText(context, id, height, row, col, width, dia, thick, topCirFace);
        unionAllSoild(context, id);
        opDeleteBodies(context, id + "deleteBodies1", {
                 "entities" : qSketchFilter(qCreatedBy(id), SketchObject.YES)
         });
       
    });
    
    /*
    **  Create the base square of the lego
    **  default unit is millimeter
    **  Parameters: baseSketch: [Sketch]sketch for the base of the lego
    **              row: [number]row of the lego = definition.Row
    **              col: [number]colum of the lego = definition.Col
    **              height: [number]height of the lego = 11.2
    **              thick: [number] thick size of the lego = 1.6
    **             
    **  Return: [Query]face of the top birck-> for creating the stub later
    */
    function createBaseLego(baseSketch is Sketch, context is Context, id is Id, row is number, width is number, col is number, height is number, thick is number)
    {
        skRectangle(baseSketch, "brickBase", {
                "firstCorner" : vector(0, 0) * millimeter,
                "secondCorner" : vector(width * row, width * col) * millimeter
        });
        skSolve(baseSketch);
        opExtrude(context, id + "baseExtrude", {
                "entities" : qSketchRegion(id + "baseSketch"),
                "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + "baseSketch")}).normal,
                "endBound" : BoundingType.BLIND,
                "endDepth" : (height - thick) * millimeter
        });
        return qNthElement(qCreatedBy(id + "baseExtrude", EntityType.FACE), 2);
    }
    
    /*
    **  Create the top stub
    **  default unit is millimeter
    **  Parameters: baseSketch: [Sketch]sketch for the base of the lego
    **              row: [number]row of the lego = definition.Row
    **              col: [number]colum of the lego = definition.Col
    **              dia: [number]radius of the stub = 4.8
    **              thick: [number] thick size of the lego = 1.6
    **             
    **  Return: [Query]face of the top stub-> for adding text later
    */
    function createTopCircles(blockFace is Query, context is Context, id is Id, row is number, width is number, col is number, dia is number, thick is number)
    {
        var sketchTopCircle = newSketch(context, id + "skTopCir", {
            "sketchPlane" : blockFace
        });
        var cirId = 0;
        for (var c = 0; c < col; c += 1)
        {
            for (var r = 0; r < row; r += 1)
            {
                skCircle(sketchTopCircle, "topCir" ~ cirId, {
                        "center" : vector((width / 2) + width * r, (width / 2) + width * c) * millimeter,
                        "radius" : dia / 2 * millimeter
                });
                cirId += 1;
            }
        }
        skSolve(sketchTopCircle);
        opExtrude(context, id + "topCirExtrude", {
                "entities" : qSketchRegion(id + "skTopCir"),
                "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + "skTopCir")}).normal,
                "endBound" : BoundingType.BLIND,
                "endDepth" : thick * millimeter
        });
        return qNthElement(qCreatedBy(id + "topCirExtrude", EntityType.FACE), 1);
    }
    
    /*
    **  function for create tube support brick up than the 2*2
    **  Hollow tube at the bottom of the lego
    **  default unit is millimeter
    **  Parameters: baseSketch: [Sketch]sketch for the base of the lego
    **              row: [number]row of the lego = definition.Row
    **              col: [number]colum of the lego = definition.Col
    **              height: [number]height of the lego = 11.2
    **              width: [number]length of the lego side = 8
    **              dia: [number]radius of the stub = 4.8
    **              thick: [number] thick size of the lego = 1.6
    **             
    **  Return: NULL
    */
    function createIinnerTube(context is Context, id is Id, row is number, col is number, height is number, width is number, thick is number, dia is number)
    {
            var tubeSketch = newSketch(context, id + "tubeSketch", {
                    "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
            var tubeCirId = 0;

            for (var c = 2; c <= col ; c += 1)
            {
                for (var r = 2; r <= row ; r += 1)
                {
                    skCircle(tubeSketch, "tube_cir" ~ tubeCirId, {
                            "center" : vector(width * (r - 1), width * (c - 1)) * millimeter,
                            "radius" : dia / 4.8 * 6.4  / 2 * millimeter
                    });
                    tubeCirId += 1;
                }
            }
            skSolve(tubeSketch);
            
            opExtrude(context, id + "tubeCirExtrude", {
                "entities" : qSketchRegion(id + "tubeSketch"),
                "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + "tubeSketch")}).normal,
                "endBound" : BoundingType.BLIND,
                "endDepth" : (height - thick) * millimeter
            });
            var tubeInSketch = newSketch(context, id + "tubeInSketch", {
                    "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
            var tubeInCirId = 0;

            for (var c = 2; c <= col ; c += 1)
            {
                for (var r = 2; r <= row ; r += 1)
                {
                    skCircle(tubeInSketch, "tubeCir" ~ tubeInCirId, {
                            "center" : vector(width * (r - 1), width * (c - 1)) * millimeter,
                            "radius" : dia / 2 * millimeter
                    });
                    tubeInCirId += 1;
                }
            }
            skSolve(tubeInSketch);
            opExtrude(context, id + "tubeInCirExtrude", {
                "entities" : qSketchRegion(id + "tubeInSketch"),
                "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + "tubeInSketch")}).normal,
                "endBound" : BoundingType.BLIND,
                "endDepth" : (height - thick) * millimeter
            });
            // Use the big hole extrusion to substract it from the part
            opBoolean(context, id + "tubeCir", {
            "tools" : qCreatedBy(id + "tubeInCirExtrude", EntityType.BODY),
            "targets" : qCreatedBy(id + "tubeCirExtrude", EntityType.BODY),
            "operationType" : BooleanOperationType.SUBTRACTION
            });
    }
        
    /*
    **  Fuction for tube that is X * 1 || 1 * X bricks
    **  Solid tube at the bottom of the lego
    **  default unit is millimeter
    **  Parameters: baseSketch: [Sketch]sketch for the base of the lego
    **              row: [number]row of the lego = definition.Row
    **              col: [number]colum of the lego = definition.Col
    **              height: [number]height of the lego = 11.2
    **              width: [number]length of the lego side = 8
    **              dia: [number]radius of the stub = 4.8
    **              thick: [number] thick size of the lego = 1.6
    **             
    **  Return: NULL
    */
    function createSolidTube(context is Context, id is Id, row is number, col is number, thick is number, width is number, dia is number, height is number)
    {
        var stubeSketch = newSketch(context, id + "stubeSketch", {
            "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
        var stube = (row == 1) ? col : row;
        var stubeCirId = 0;

        for (var count = 1; count < stube ; count += 1)
        {
            var c = (row == 1) ? count - 1 : 0;
            var r = (row == 1) ? 0 : count - 1;
            var counter_c = (row == 1) ? 1 : 2;
            var counter_r = (row == 1) ? 2 : 1;
            skCircle(stubeSketch, "tubeCir" ~ stubeCirId, {
                    "center" : vector(width / counter_r + width * r, width / counter_c + width * c) * millimeter,
                    "radius" : (dia - thick)  / 2 * millimeter
            });
            stubeCirId += 1;
        }
        skSolve(stubeSketch);
        
        opExtrude(context, id + "stubeCirExtrude", {
            "entities" : qSketchRegion(id + "stubeSketch"),
            "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + "stubeSketch")}).normal,
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
    function unionAllSoild(context is Context, id is Id)
    {
        opBoolean(context, id + "unionAll", {
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
    function shellTheLego(context is Context, id is Id, thick is number, shellFace is Query)
    {
        opShell(context, id + "shellBase", {
                "entities" : shellFace,
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
    function createLegoText(context is Context, id is Id, height is number, row is number, col is number, width is number, dia is number, thick is number, topCirFace is Query){
        var skText1 = newSketch(context, id + "skText", {
                "sketchPlane" : topCirFace
        });
        var textId = 0;
        for (var c = 1; c <= col; c += 1)
        {
            for (var r = 1; r <= row; r += 1)
            {
                skText(skText1, "text1" ~textId, {
                    "text" : "LEGO",
                    "fontName" : "OpenSans-Regular.ttf",
                    "firstCorner" : vector((width / 2 * sqrt(2) - dia *0.80) + width * (r - 1), (width / 2 * sqrt(2) - dia *0.50) + width * (c - 1)) * millimeter,
                    "secondCorner" : vector((width / 2 * sqrt(2) - dia *0.80) + dia * 0.10 + width * (r - 1), (width / 2 * sqrt(2) - dia *0.50) + dia * 0.25 + width * (c - 1)) * millimeter
                });
                textId += 1;
            }
        }
        skSolve(skText1);
        opExtrude(context, id + "extrudeText", {
                "entities" : qSketchRegion(id + "skText", true),
                "direction" : evOwnerSketchPlane(context, {"entity" : qSketchRegion(id + "skText")}).normal,
                "endBound" : BoundingType.BLIND,
                "endDepth" : thick  / 8 * millimeter
        });

    }
    
        

