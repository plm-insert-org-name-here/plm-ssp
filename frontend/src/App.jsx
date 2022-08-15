import useResizeObserver from "@react-hook/resize-observer";
import axios from "axios";
import { useSnackbar } from "notistack";
import { useRef, useEffect, useState, createContext, useReducer } from "react";
import { BrowserRouter, Route, Switch } from "react-router-dom";

import HomeIcon from "@mui/icons-material/Home";
import { AppBar } from "@mui/material";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Grid from "@mui/material/Grid";
import Typography from "@mui/material/Typography";
import { useTheme } from "@mui/material/styles";
import useMediaQuery from "@mui/material/useMediaQuery";

import "./App.css";
import About from "./components/about/About";
import OverflowText from "./components/common/OverflowText";
import Dashboard from "./components/dashboard/Dashboard";
import Infrastructure from "./components/infra/Infrastructure";

// TODO(rg): responsive
const viewportBreakpoint = "md";

const setupAxiosInterceptors = (
    onBadRequest,
    onUnauthorized,
    onForbidden,
    onInternalServerError
) => {
    axios.interceptors.response.use(
        (response) => response,
        (error) => {
            if (error.response?.status === 400) onBadRequest(error);
            if (error.response?.status === 401) onUnauthorized();
            if (error.response?.status === 403) onForbidden();
            if (error.response?.status === 500) onInternalServerError();
            return Promise.reject(error);
        }
    );
};

const defaultSelection = { locationId: null, detectorId: null };

export const InfrastructureContext = createContext(defaultSelection);

const selectionReducer = (state, { type, payload }) => {
    switch (type) {
        case "SET_LOCATION_AND_DETECTOR": {
            const { locationId, detectorId } = payload;
            return { locationId, detectorId };
        }
        default: {
            console.log("default");
            return state;
        }
    }
};

function App() {
    const { enqueueSnackbar } = useSnackbar();
    const [selection, dispatchSelection] = useReducer(selectionReducer, defaultSelection);
    const theme = useTheme();
    const isSmallScreen = useMediaQuery(theme.breakpoints.down(viewportBreakpoint));

    useEffect(() => {
        setupAxiosInterceptors(
            (err) => {
                // TODO(rg): error formatting
                enqueueSnackbar(err.response.data.status, { variant: "error" });
                console.log(err);
            },
            () => {
                // not logged in
                enqueueSnackbar("Your session has expired. Please sign in again", {
                    variant: "info",
                });
            },
            () => {
                // logged in but insufficient roles
                enqueueSnackbar("You do not have sufficient permissions to perform this action", {
                    variant: "error",
                });
            },
            () => {
                enqueueSnackbar("An internal server error occurred", { variant: "error" });
            }
        );
    }, []);

    return (
        <InfrastructureContext.Provider value={{ selection, dispatchSelection }}>
            <AppBar />
            <BrowserRouter>
                <Switch>
                    <Route path="/" exact>
                        {isSmallScreen ? (
                            <Box
                                display="flex"
                                justifyContent="center"
                                alignItems="center"
                                fontFamily="monospace"
                                fontSize="24px"
                                textAlign="center"
                                sx={{
                                    color: "white",
                                    bgcolor: "black",
                                    height: "100vh",
                                    width: "100vw",
                                }}
                            >
                                Viewport width is too small!
                                <br />
                                Need: {theme.breakpoints.values[viewportBreakpoint]} px
                            </Box>
                        ) : (
                            <Box display="flex" flexDirection="row" sx={{ p: 2, height: "100vh" }}>
                                <Box
                                    display="flex"
                                    flexDirection="column"
                                    flexGrow={0}
                                    pr={2}
                                    pb={0}
                                >
                                    <Button
                                        variant="contained"
                                        sx={{
                                            display: "flex",
                                            justifyContent: "space-around",
                                            mr: 0,
                                            mb: 2,
                                            alignSelf: isSmallScreen && "start",
                                        }}
                                    >
                                        <HomeIcon fontSize="large" sx={{ mr: 1 }} />
                                        <Box sx={{ width: 0, flexGrow: 1 }}>
                                            <OverflowText
                                                text="Station 1234 yada yada"
                                                sx={{ fontSize: "1.1rem", fontWeight: "bold" }}
                                            ></OverflowText>
                                            <Typography sx={{ fontSize: "0.6rem" }}>
                                                (Click to select)
                                            </Typography>
                                        </Box>
                                    </Button>
                                    <Infrastructure />
                                </Box>
                                <Dashboard />
                            </Box>
                        )}
                    </Route>
                    <Route path="/about" render={() => <About />} />
                </Switch>
            </BrowserRouter>
        </InfrastructureContext.Provider>
    );
}

export default App;
