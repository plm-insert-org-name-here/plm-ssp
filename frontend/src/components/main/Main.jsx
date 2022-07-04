import React, { useContext } from "react";

import { Paper } from "@mui/material";

import { InfrastructureContext } from "../../App";
import DeviceStream from "../stream/DeviceStream";

const Main = () => {
    const { selection, _ } = useContext(InfrastructureContext);

    return (
        <Paper elevation={8} sx={{ m: 2, flexGrow: 1 }}>
            {selection.detectorId && <DeviceStream enabled detectorId={selection.detectorId} />}
        </Paper>
    );
};

export default Main;
