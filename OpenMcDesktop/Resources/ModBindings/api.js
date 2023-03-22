export function setPaused(value) {
    glue.SetPaused(value)
}

export function getPaused() {
    return glue.GetPaused()
}

export function createMenu(menuId) {
    return glue.CreateMenu(menuId)
}

export function addTitle(ttle) {
    return glue.AddTitle(ttle)
}

export function addButton(menuId, text) {
    return glue.AddButton(menuId, text)
}

export function addLabel(menuID, text, colour = "#FFFFFF", fontSize = 24) {
    return glue.AddLabel(menuID, text, colour, fontSize)
}

export function deleteMenu(menuId) {
    return glue.DeleteMenu(menuId)
}