// OpenMC-Specific API components
export function getBlocks() {
    return glue.GetBlocks()
}

export function getEntities() {
    return glue.GetEntities()
}

export function getItems() {
    return glue.GetItems()
}

export function defineBlock(blockInfo) {
    glue.DefineBlock(blockInfo)
}

export function undefineBlock() {
    glue.UndefineBlock()
}

export function defineItem() {
    glue.DefineItem()
}

export function undefineItem() {
    glue.UndefineItem()
}

export function defineEntity() {
    glue.DefineEntity()
}

export function undefineEntity() {
    glue.UndefineEntity()
}