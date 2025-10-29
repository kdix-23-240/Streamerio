function doGet(e) {
    const dataType = e.parameter.dataType;

    var jsonData;
    switch(dataType) {
        case sheetName.PLAYER_STATUS:
            jsonData = createJson(sheetName, playerStatusInitRowIndex, playerStatusProp);
    }

    console.log(jsonData);

    return createOutput(jsonData);
}