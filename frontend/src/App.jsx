import axios from "axios";
import { useSnackbar } from "notistack";
import { useEffect, useState } from "react";
import { BrowserRouter, Route, Switch } from "react-router-dom";

import { Grid } from "@mui/material";

import "./App.css";
import AppBar from "./common/appbar/AppBar";
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

function App() {
    const { enqueueSnackbar } = useSnackbar();
    const [selectedLocation, setSelectedLocation] = useState(null);

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
        <>
            <AppBar />
            <BrowserRouter>
                <Switch>
                    <Route path="/" exact>
                        <Grid container sx={{ p: 1, height: "100vh" }}>
                            <Grid item xs={12} md={4} lg={3} display="flex">
                                <Infrastructure
                                    selectedLocation={selectedLocation}
                                    setSelectedLocation={setSelectedLocation}
                                />
                            </Grid>
                            <Grid item xs={12} md={8} lg={9} display="flex">
                                <Main />
                            </Grid>
                        </Grid>
                    </Route>
                    <Route path="/about" render={() => <About />} />
                </Switch>
            </BrowserRouter>
        </>
    );
}

export default App;
