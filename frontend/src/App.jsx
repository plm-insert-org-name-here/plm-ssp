import axios from "axios";
import { useSnackbar } from "notistack";
import { useEffect, useState, createContext, useReducer } from "react";
import { BrowserRouter, Route, Switch } from "react-router-dom";

import { AppBar } from "@mui/material";
import Grid from "@mui/material/Grid";

import "./App.css";
import About from "./components/about/About";
import Infrastructure from "./components/infra/Infrastructure";
import Main from "./components/main/Main";

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
            console.log(`setting to ${locationId} / ${detectorId}`);
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
                        <Grid container sx={{ p: 1, height: "100vh" }}>
                            <Grid item xs={12} md={4} lg={3} display="flex">
                                <Infrastructure />
                            </Grid>
                            <Grid item xs={12} md={8} lg={9} display="flex">
                                <Main />
                            </Grid>
                        </Grid>
                    </Route>
                    <Route path="/about" render={() => <About />} />
                </Switch>
            </BrowserRouter>
        </InfrastructureContext.Provider>
    );
}

export default App;
