import React, { useContext, useEffect, useRef } from "react";

import AccessTimeIcon from "@mui/icons-material/AccessTime";
import EditIcon from "@mui/icons-material/Edit";
import GridViewIcon from "@mui/icons-material/GridView";
import PlayIcon from "@mui/icons-material/PlayArrow";
import { Fab, IconButton, Paper, Typography } from "@mui/material";
import Box from "@mui/material/Box";
import Divider from "@mui/material/Divider";
import Grid from "@mui/material/Grid";
import LinearProgress from "@mui/material/LinearProgress";

import { InfrastructureContext } from "../../App";
import LabeledValues from "../common/LabeledValues.jsx";
import OverflowText from "../common/OverflowText.jsx";
import DetectorStream from "../stream/DetectorStream.jsx";
import DeviceStream from "../stream/DeviceStream";
import StatusPanel from "./StatusPanel.jsx";

const Dashboard = () => {
    const { selection, _ } = useContext(InfrastructureContext);

    return (
        <Paper elevation={8} sx={{ flexGrow: 1 }}>
            <Box sx={{ height: "48px" }}>header</Box>
            <Divider />
            <Grid container height="calc(100% - 48px)">
                <Grid
                    item
                    display="flex"
                    flexDirection="column"
                    xs={8}
                    sx={{
                        height: "100%",
                        borderRadius: "0 0 0 8px",
                        p: 2,
                    }}
                >
                    <Box sx={{ height: "calc(100% - 200px)" }}>
                        <DetectorStream />
                    </Box>
                    <Box
                        sx={{
                            pt: 2,
                            display: "flex",
                            height: "200px",
                            gap: 4,
                        }}
                    >
                        <StatusPanel
                            title="Stream"
                            width_ratio={1}
                            stats={<LabeledValues values={[{ label: "FPS", inner: "24" }]} />}
                            actions={
                                <>
                                    <Fab size="small" color="primary">
                                        <PlayIcon />
                                    </Fab>
                                </>
                            }
                        />
                        <StatusPanel
                            title="Monitor"
                            width_ratio={2}
                            details={
                                <>
                                    <OverflowText text="job1/task1/template1" variant="overline" />
                                </>
                            }
                            stats={
                                <LabeledValues
                                    values={[
                                        {
                                            icon: (
                                                <AccessTimeIcon
                                                    sx={{
                                                        height: "24px",
                                                        mb: "4px",
                                                        verticalAlign: "middle",
                                                    }}
                                                />
                                            ),
                                            label: "Total",
                                            inner: "00:42",
                                        },
                                        {
                                            icon: <AccessTimeIcon />,
                                            label: "Current",
                                            inner: "00:11",
                                        },
                                        {
                                            icon: <GridViewIcon />,
                                            label: "Progress",
                                            inner: "2 / 8",
                                        },
                                        {
                                            label: "Type",
                                            inner: "item kit",
                                        },
                                    ]}
                                />
                            }
                            actions={
                                <>
                                    <Fab size="small" color="primary">
                                        <PlayIcon />
                                    </Fab>
                                    <Fab size="small" color="secondary">
                                        <EditIcon />
                                    </Fab>
                                </>
                            }
                        />
                    </Box>
                </Grid>
                <Grid
                    item
                    xs={4}
                    sx={{
                        backgroundColor: "lightblue",
                        height: "100%",
                        borderRadius: "0 0 8px 0",
                    }}
                >
                    x
                </Grid>
            </Grid>
            {/*  selection?.detectorId && <DeviceStream enabled detectorId={selection.detectorId} /> */}
        </Paper>
    );
};

export default Dashboard;
