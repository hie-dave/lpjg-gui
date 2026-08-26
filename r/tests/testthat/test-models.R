test_that("factor constructors use protocol values", {
    expect_equal(top_level_parameter("npatch", 12)$value, "12")
    expect_equal(top_level_parameter("ifcentury", TRUE)$value, "true")

    block <- block_parameter("pft", "MRS", "sla", 26.5)
    expect_equal(block$type, "block")
    expect_equal(block$block_name, "MRS")
    expect_equal(block$value, "26.5")
})

test_that("simulation accepts individual factors or a list", {
    factor <- top_level_parameter("npatch", 2)
    expect_equal(simulation("a", factor)$factors, list(factor))
    expect_equal(simulation("a", list(factor))$factors, list(factor))
})
