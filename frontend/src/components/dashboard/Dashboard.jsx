import React, { useContext } from "react";

import { Paper } from "@mui/material";
import Grid from "@mui/material/Grid";

import { InfrastructureContext } from "../../App";
import DeviceStream from "../stream/DeviceStream";

const Dashboard = () => {
    const { selection, _ } = useContext(InfrastructureContext);

    return (
        <Paper elevation={8} sx={{ m: 2, flexGrow: 1 }}>
            <Grid container height="100%">
                <Grid item xs={8} sx={{ height: "100%", borderRadius: "8px 0 0 8px" }}>
                    x
                </Grid>
                <Grid item xs={4} sx={{ height: "100%", borderRadius: "0 8px 8px 0" }}>
                    x
                </Grid>
            </Grid>
            {selection?.detectorId && <DeviceStream enabled detectorId={selection.detectorId} />}
        </Paper>
    );
};

export default Dashboard;
