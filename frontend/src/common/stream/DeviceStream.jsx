import axios from "axios";
import { decode } from "base64-arraybuffer";
import { bindPopover, bindTrigger, usePopupState } from "material-ui-popup-state/hooks";
import { useSnackbar } from "notistack";
import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { CSSTransition } from "react-transition-group";

import MoreHorizIcon from "@mui/icons-material/MoreHoriz";
import PanoramaWideAngleIcon from "@mui/icons-material/PanoramaWideAngle";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import StopIcon from "@mui/icons-material/Stop";
import { LoadingButton } from "@mui/lab";
import { Checkbox, FormControlLabel, FormGroup, Popover } from "@mui/material";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Grid from "@mui/material/Grid";
import IconButton from "@mui/material/IconButton";

import { streamDrawFps } from "../canvas/utils";
import useMounted from "../hooks/useMounted";
import "./devices.css";
import useSignalR from "./useSignalR";

const placeholderImageSrc = "https://via.placeholder.com/640x480";

const DeviceStream = ({ enabled, deviceId, setSnapshot }) => {
    // NOTE(rg): this is not necessary, but it avoids a deprecation error related to findDOMNode,
    // used internally by the CSSTransition component
    // source: https://github.com/reactjs/react-transition-group/issues/668#issuecomment-695162879
    const transitionNodeRef = useRef(null);
    const canvasRef = useRef();
    const menuPopup = usePopupState({ variant: "popover", popupId: "stream-menu" });

    const [drawFps, setDrawFps] = useState(true);
    const [isControlDisabled, setControlDisabled] = useState(false);

    const [active, setActive] = useState(false);
    const [streamSrc, setStreamSrc] = useState(null);
    const [snapshotSrc, setSnapshotSrc] = useState(null);
    const [snapshotLoading, setSnapshotLoading] = useState(false);
    const { enqueueSnackbar } = useSnackbar();

    const isMounted = useMounted();

    const fpsRef = useRef();
    const frameRef = useRef();

    useEffect(() => {
        const handler = (e) => {
            if (active) {
                e.preventDefault();

                handleStopStream();
            }
        };

        window.addEventListener("beforeunload", handler);
        return () => window.removeEventListener("beforeunload", handler);
    }, [active]);

    useEffect(() => {
        if (active) return;

        // NOTE(rg): if we're immediately clearing the canvas when stopping the stream,
        // the last frame will arrive after the canvas is cleared and will overwrite it,
        // hence the timeout
        window.setTimeout(() => {
            const canvas = canvasRef.current;
            const ctx = canvas.getContext("2d");

            ctx.clearRect(0, 0, canvas.width, canvas.height);
        }, 500);
    }, [active]);

    const handleTakeSnapshot = async () => {
        await axios.post(Endpoints.streamSnapshot(deviceId));
        setSnapshotLoading(true);
    };

    // Do a complete redraw each time
    //  - new frame is received (may lag)
    //  - a new FPS value is received (guaranteed every second)
    const draw = () => {
        if (!frameRef.current) return;
        const base64 = frameRef.current.base64;

        const arrayBuffer = decode(base64);
        // release the previous frame in order to not leak memory
        if (streamSrc) {
            URL.revokeObjectURL(streamSrc);
        }

        const blob = new Blob([arrayBuffer], { type: "image/jpeg" });
        const url = URL.createObjectURL(blob);
        const image = new Image();
        image.src = url;
        image.onload = () => {
            const canvas = canvasRef.current;
            if (!canvas) return;
            canvas.width = image.width;
            canvas.height = image.height;

            const ctx = canvas.getContext("2d");
            ctx.drawImage(image, 0, 0);

            if (drawFps && fpsRef.current !== undefined) {
                streamDrawFps(ctx, fpsRef.current);
            }
        };

        if (isMounted()) {
            setStreamSrc(url);
        }
    };

    const handleFps = (fps) => {
        fpsRef.current = fps;
        draw();
    };

    const handleReceiveStream = (base64) => {
        if (active && !snapshotLoading) {
            frameRef.current = { base64 };
            draw();
        }
    };

    const handleReceiveSnapshot = (base64) => {
        // release the previous frame in order to not leak memory
        if (snapshotSrc) {
            URL.revokeObjectURL(snapshotSrc);
        }

        const arrayBuffer = decode(base64);
        const blob = new Blob([arrayBuffer], { type: "image/bmp" });
        const url = URL.createObjectURL(blob);
        setSnapshotSrc(url);
        setActive(false);
        setSnapshotLoading(false);
        setSnapshot && setSnapshot(arrayBuffer);
        enqueueSnackbar("Snapshot taken", { variant: "success" });
    };

    const handleStartStream = async () => {
        setActive(true);
        setControlDisabled(true);

        window.setTimeout(() => {
            setControlDisabled(false);
        }, 1000);
    };

    const handleStopStream = () => {
        setActive(false);
        setControlDisabled(true);

        window.setTimeout(() => {
            setControlDisabled(false);
        }, 1000);
    };

    const handlers = useMemo(() => {
        return [
            ["Streaming", handleReceiveStream],
            ["Snapshot", handleReceiveSnapshot],
            ["Fps", handleFps],
        ];
    }, [active, drawFps]);

    useSignalR({
        url: Endpoints.signalrConnection,
        active: active,
        groups: ["Stream-" + deviceId, "Snapshot-" + deviceId],
        handlers: handlers,
    });

    return (
        <>
            <Grid
                container
                flexDirection="column"
                alignItems="center"
                justifyContent="center"
                gap={2}
            >
                <Box
                    sx={{
                        position: "relative",
                        display: "flex",
                        flex: 1,
                    }}
                >
                    <Box
                        sx={{
                            position: "absolute",
                            width: "100%",
                            height: "100%",
                            boxShadow: 4,
                            bgcolor: "black",
                            zIndex: 1,
                        }}
                        component="canvas"
                        ref={canvasRef}
                    />
                    <CSSTransition
                        nodeRef={transitionNodeRef}
                        in={active}
                        timeout={500}
                        classNames="snapshot-image"
                    >
                        <Box
                            ref={transitionNodeRef}
                            sx={{ zIndex: 2 }}
                            component="img"
                            src={snapshotSrc ?? placeholderImageSrc}
                            alt="Stream"
                        />
                    </CSSTransition>
                </Box>
                <Grid
                    item
                    sx={{ display: "flex", alignItems: "center", gap: 4, justifyContent: "center" }}
                >
                    {active ? (
                        <>
                            <Button
                                disabled={!enabled || snapshotLoading || isControlDisabled}
                                variant="contained"
                                color="error"
                                startIcon={<StopIcon />}
                                onClick={handleStopStream}
                            >
                                Stop stream
                            </Button>
                            {setSnapshot ? (
                                <LoadingButton
                                    disabled={isControlDisabled}
                                    loading={snapshotLoading}
                                    loadingPosition="start"
                                    variant="contained"
                                    startIcon={<PanoramaWideAngleIcon />}
                                    onClick={handleTakeSnapshot}
                                >
                                    Take snapshot
                                </LoadingButton>
                            ) : null}
                        </>
                    ) : (
                        <Button
                            disabled={!enabled || isControlDisabled}
                            variant="contained"
                            color="primary"
                            startIcon={<PlayArrowIcon />}
                            onClick={handleStartStream}
                        >
                            Start stream
                        </Button>
                    )}
                    <IconButton {...bindTrigger(menuPopup)}>
                        <MoreHorizIcon />
                    </IconButton>
                </Grid>
            </Grid>
            <Popover
                {...bindPopover(menuPopup)}
                anchorOrigin={{
                    vertical: "top",
                    horizontal: "center",
                }}
                transformOrigin={{
                    vertical: "bottom",
                    horizontal: "center",
                }}
            >
                <FormGroup sx={{ px: 1 }}>
                    <FormControlLabel
                        control={
                            <Checkbox
                                checked={drawOverlay}
                                onChange={(e) => setDrawOverlay(e.target.checked)}
                            />
                        }
                        label="Draw overlay"
                    />
                    <FormControlLabel
                        control={
                            <Checkbox
                                checked={drawLabels}
                                onChange={(e) => setDrawLabels(e.target.checked)}
                            />
                        }
                        label="Draw labels"
                    />
                    <FormControlLabel
                        control={
                            <Checkbox
                                checked={drawFps}
                                onChange={(e) => setDrawFps(e.target.checked)}
                            />
                        }
                        label="Draw FPS"
                    />
                </FormGroup>
            </Popover>
        </>
    );
};

export default DeviceStream;
