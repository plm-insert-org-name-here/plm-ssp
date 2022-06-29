import React from "react";
import ReactDOM from "react-dom";

import App from "./App";
import {CssBaseline, Grow} from "@mui/material";
import {SnackbarProvider} from "notistack";
import {ThemeProvider} from "@mui/material";
import theme from "./theme";

ReactDOM.render(
    <React.StrictMode>
        <ThemeProvider theme={theme}>
            <SnackbarProvider maxSnack={3} TransitionComponent={Grow}>
                <CssBaseline/>
                <App/>
            </SnackbarProvider>
        </ThemeProvider>
    </React.StrictMode>,
    document.getElementById("root")
);
