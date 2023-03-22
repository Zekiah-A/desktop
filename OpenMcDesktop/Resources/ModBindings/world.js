// OpenMC-Specific API components
export function setblock(x, y, b) {
    glue.SetBlock()
}

export function getblock(x, y) {
    return glue.GetBlock()
}

export function getAllPlayers() {
    return glue.GetAllPlayers()
}

export function addEntity(entity) {
    glue.AddEntity()
}

export function removeEntity(entity) {
    glue.RemoveEntity()
}

export function sendMessage(message) {
    glue.SendMessage()
}