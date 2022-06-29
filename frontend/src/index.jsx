import { SnackbarProvider } from "notistack";
import React from "react";
import ReactDOM from "react-dom";

import { CssBaseline, Grow, ThemeProvider } from "@mui/material";

import App from "./App";
import theme from "./theme";

ReactDOM.render(
    <React.StrictMode>
        <ThemeProvider theme={theme}>
            <SnackbarProvider maxSnack={3} TransitionComponent={Grow}>
                <CssBaseline />
                <App />
            </SnackbarProvider>
        </ThemeProvider>
    </React.StrictMode>,
    document.getElementById("root")
);
