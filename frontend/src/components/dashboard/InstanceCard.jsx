import React, { useState } from "react";

import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import Collapse from "@mui/material/Collapse";
import IconButton from "@mui/material/IconButton";
import { useTheme } from "@mui/material/styles";

const InstanceCard = () => {
    const [expanded, setExpanded] = useState(false);
    const theme = useTheme();
    // transition: theme.transitions.create('transform'o, {
    //   duration: theme.transitions.duration.shortest,

    return (
        <Card
            elevation={1}
            sx={{ minHeight: "100px", ml: 2, mr: 3, my: 1, display: "flex", borderRadius: "4px" }}
        >
            <IconButton
                sx={{
                    ml: "auto",
                    mt: "auto",
                    transform: expanded && "rotate(180deg)",
                    transition: theme.transitions.create("transform", {
                        duration: theme.transitions.duration.shortest,
                    }),
                }}
                onClick={() => setExpanded((e) => !e)}
            >
                <ExpandMoreIcon />
            </IconButton>
            <Collapse in={expanded} timeout="auto" easing="easeInOut" unmountOnExit>
                <Box sx={{ bgcolor: "lightblue", height: "200px" }}></Box>
            </Collapse>
        </Card>
    );
};

export default InstanceCard;
