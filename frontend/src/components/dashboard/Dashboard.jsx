import React, { useState, useContext, useEffect, useRef } from "react";

import AccessTimeIcon from "@mui/icons-material/AccessTime";
import EditIcon from "@mui/icons-material/Edit";
import GridViewIcon from "@mui/icons-material/GridView";
import PlayIcon from "@mui/icons-material/PlayArrow";
import StopIcon from "@mui/icons-material/Stop";
import { Fab, IconButton, Paper, Typography } from "@mui/material";
import Box from "@mui/material/Box";
import Divider from "@mui/material/Divider";
import Grid from "@mui/material/Grid";
import LinearProgress from "@mui/material/LinearProgress";

import { InfrastructureContext } from "../../App";
import LabeledValues from "../common/LabeledValues.jsx";
import OverflowText from "../common/OverflowText.jsx";
import DetectorStream from "../stream/DetectorStream.jsx";
import StatusPanel from "./StatusPanel.jsx";

const Dashboard = () => {
    const [streamActive, setStreamActive] = useState(false);
    const [streamFps, setStreamFps] = useState(0);
    const [streamControlsDisabled, setStreamControlsDisabled] = useState(false);
    const { selection, _ } = useContext(InfrastructureContext);

    // Close the stream on exiting the page. If the stream is not closed, the stream subscriber
    // count on the backend gets messed up
    useEffect(() => {
        const handler = (e) => {
            if (streamActive) {
                e.preventDefault();
                onStopStream();
            }
        };

        window.addEventListener("beforeunload", handler);
        return () => window.removeEventListener("beforeunload", handler);
    }, [streamActive]);

    const onStartStream = async () => {
        setStreamActive(true);
        setStreamControlsDisabled(true);

        window.setTimeout(() => {
            setStreamControlsDisabled(false);
        }, 1000);
    };

    const onStopStream = async () => {
        setStreamActive(false);
        setStreamControlsDisabled(true);

        window.setTimeout(() => {
            setStreamControlsDisabled(false);
        }, 1000);
    };

    if (!selection.detectorId) {
        return "";
    }

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
                        <DetectorStream
                            setFps={setStreamFps}
                            active={streamActive}
                            detectorId={selection?.detectorId}
                        />
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
                            stats={
                                streamActive && (
                                    <LabeledValues values={[{ label: "FPS", inner: streamFps }]} />
                                )
                            }
                            actions={
                                <>
                                    {!streamActive ? (
                                        <Fab
                                            onClick={onStartStream}
                                            size="small"
                                            color="primary"
                                            disabled={streamControlsDisabled}
                                        >
                                            <PlayIcon />
                                        </Fab>
                                    ) : (
                                        <Fab
                                            onClick={onStopStream}
                                            size="small"
                                            color="error"
                                            disabled={streamControlsDisabled}
                                        >
                                            <StopIcon />
                                        </Fab>
                                    )}
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
        </Paper>
    );
};

export default Dashboard;
