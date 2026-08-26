# lpjg-experiment

The experiment runner supports a human-facing TOML mode and a versioned
machine mode. Both modes execute through `ExperimentRunner`.

```sh
lpjg-experiment experiment.toml
lpjg-experiment run experiment.toml
```

Machine clients pass a protocol-v1 JSON request and receive one JSON event per
line on stdout. Diagnostics which are not protocol events are written to
stderr.

```sh
lpjg-experiment run --request-json request.json --events ndjson
```

Use `-` as the request path to read JSON from stdin. Event types are `started`,
`progress`, `output`, `completed`, `cancelled`, and `error`. `output` events
identify the originating job and whether the content came from stdout or
stderr. Ctrl-C is translated to runner cancellation and returns exit status
130.

The request supports local and PBS run settings, explicitly named simulations,
top-level and block parameters, instruction files, PFT selection, and combined
existing-output policies. See the R package for a complete request builder.
