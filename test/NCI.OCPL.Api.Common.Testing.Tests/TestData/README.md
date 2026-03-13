
## test-response.json

    This file contains results for autosuggest
    based on the keyword "breast"

    ```json
    POST /autosg/terms/_search/template
    {
        "file" : "autosg_suggest_cgov_en",
        "params" : {
            "searchstring" : "breast",
            "my_size" : 20
        }
    }
    ```