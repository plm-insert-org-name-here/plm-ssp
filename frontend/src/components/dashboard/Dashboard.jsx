import useResizeObserver from "@react-hook/resize-observer";
import React, { useContext, useRef, useEffect } from "react";

import { Paper } from "@mui/material";
import Box from "@mui/material/Box";
import Divider from "@mui/material/Divider";
import Grid from "@mui/material/Grid";

import { InfrastructureContext } from "../../App";
import DeviceStream from "../stream/DeviceStream";

const Dashboard = () => {
    const { selection, _ } = useContext(InfrastructureContext);
    const canvasRef = useRef();
    const canvasContainerRef = useRef();

    useResizeObserver(canvasContainerRef, (_) => {
        console.count("canvas container resize");
        const canvas = canvasRef.current;
        const canvasContainer = canvasContainerRef.current;
        draw(canvas, canvasContainer);
    });

    const draw = (canvas, canvasContainer) => {
        const contw = canvasContainer.clientWidth;
        const conth = canvasContainer.clientHeight;
        const ardiff = contw / conth - 4 / 3;

        if (ardiff > 0) {
            canvas.style.width = "auto";
            canvas.style.height = conth + "px";
        } else {
            canvas.style.width = contw + "px";
            canvas.style.height = "auto";
        }
        canvas.width = 640;
        canvas.height = 480;

        // draw checker pattern
        const ctx = canvas.getContext("2d");
        ctx.fillStyle = "#a22";
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        ctx.fillStyle = "#22a";
        const rect_size = 20;
        for (let j = 0; j < Math.ceil(canvas.height / rect_size); j++) {
            for (let i = 0; i < Math.ceil(canvas.width / rect_size); i++) {
                if ((j + i) % 2 === 1) {
                    ctx.fillRect(
                        i * rect_size,
                        j * rect_size,
                        Math.min(rect_size, canvas.width - i * rect_size),
                        Math.min(rect_size, canvas.height - j * rect_size)
                    );
                }
            }
        }
    };

    useEffect(() => {
        const canvas = canvasRef.current;
        const canvasContainer = canvasContainerRef.current;

        draw(canvas, canvasContainer);
    }, []);

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
                        backgroundColor: "pink",
                        height: "100%",
                        borderRadius: "0 0 0 8px",
                    }}
                >
                    <Box
                        ref={canvasContainerRef}
                        display="flex"
                        m={2}
                        justifyContent="center"
                        alignItems="center"
                        sx={{
                            backgroundColor: "black",
                            height: "calc(100% - 200px)",
                        }}
                    >
                        <Box
                            component="canvas"
                            ref={canvasRef}
                            sx={{ maxHeight: "100%", maxWidth: "100%", aspectRatio: "4/3" }}
                        />
                    </Box>
                    <Box sx={{ minHeight: "200px", backgroundColor: "lightgreen" }}>x</Box>
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
