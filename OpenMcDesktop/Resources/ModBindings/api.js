
export let W2 = 0, H2 = 0, W = 0, H = 0, SCALE = 1

export let paused = () => glue.GetPaused()
export let pause = () => glue.setPaused(true)
export function _setPaused(value) { glue.setPaused(value) }

