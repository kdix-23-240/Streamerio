const spreadSheet = SpreadsheetApp.openById(getEnvParam('SPREAD_SHEET_ID'));

const sheetName = {
    PLAYER_STATUS: 'PlayerStatus',
}

function getSheet(sheetName) {
    return spreadSheet.getSheetByName(sheetName);
}

function getRows(sheetName, initRowIndex, columnIndex) {
    const sheet = getSheet(sheetName);
    const lastRow = sheet.getLastRow();
    if (initRowIndex > lastRow) {
        console.warn(`Requested row index ${initRowIndex} exceeds last row ${lastRow} in sheet ${sheetName}.`);
        return [];
    }

    return sheet.getRange(initRowIndex, columnIndex, lastRow-(initRowIndex-1), 1).getValues();
}