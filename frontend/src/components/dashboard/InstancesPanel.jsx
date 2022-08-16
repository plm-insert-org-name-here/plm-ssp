import React, { useState } from "react";

import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardActionArea from "@mui/material/CardActionArea";
import Divider from "@mui/material/Divider";
import Typography from "@mui/material/Typography";

import InstanceCard from "./InstanceCard.jsx";

const InstancesPanel = () => {
    const [selectedTab, setSelectedTab] = useState(0);
    return (
        <Box
            sx={{
                height: "100%",
                display: "flex",
                flexGrow: 1,
                flexDirection: "column",
                borderRadius: "8px",
                border: "1px solid",
                borderColor: "divider",
            }}
        >
            <Typography variant="overline" sx={{ p: 2, lineHeight: 1, fontSize: "1.1rem" }}>
                Results
            </Typography>
            <Divider />
            <Box
                sx={{
                    bgcolor: "background.panel",
                    gap: 1,
                    flexGrow: 1,
                    overflowY: "auto",
                }}
            >
                {Array(20)
                    .fill("")
                    .map((_, i) => (
                        <InstanceCard key={i} />
                    ))}
            </Box>
        </Box>
    );
};

export default InstancesPanel;
