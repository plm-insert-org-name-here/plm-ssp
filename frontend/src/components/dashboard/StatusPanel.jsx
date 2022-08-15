import React from "react";

import PlayIcon from "@mui/icons-material/PlayArrow";
import Box from "@mui/material/Box";
import Fab from "@mui/material/Fab";
import Typography from "@mui/material/Typography";

const StatusPanel = ({ width_ratio, title, details, stats, actions }) => {
    return (
        <Box
            sx={{
                display: "flex",
                flexDirection: "column",
                bgcolor: "background.panel",
                borderRadius: "8px",
                flex: width_ratio,
                p: 2,
            }}
        >
            <Box
                sx={{
                    width: "100%",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "space-between",
                }}
            >
                <Typography variant="overline" sx={{ lineHeight: 1, fontSize: "1.1rem" }}>
                    {title}
                </Typography>
                <Box display="flex" gap={2}>
                    {actions}
                </Box>
            </Box>
            <Box
                sx={{
                    width: "100%",
                    display: "flex",
                    alignItems: "center",
                    gap: 4,
                }}
            >
                {details}
            </Box>
            <Box sx={{ mt: "auto" }}>{stats}</Box>
        </Box>
    );
};

export default StatusPanel;
