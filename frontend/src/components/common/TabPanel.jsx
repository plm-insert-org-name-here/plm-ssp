import React from "react";

import Box from "@mui/material/Box";

const TabPanel = ({ children, value, index, ...other }) => {
    return (
        <Box flex={1} display={value === index ? "flex" : "none"} flexDirection="column" {...other}>
            {children}
        </Box>
    );
};

export default TabPanel;
