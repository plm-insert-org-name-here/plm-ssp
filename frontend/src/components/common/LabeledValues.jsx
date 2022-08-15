import React from "react";

import Box from "@mui/material/Box";
import Divider from "@mui/material/Divider";
import Typography from "@mui/material/Typography";

const LabeledValues = ({ values }) => {
    return (
        <Box display="flex" gap={2}>
            {values.map((v, i) => (
                <>
                    <Box key={i} display="flex" flexDirection="column" alignItems="center">
                        <Typography variant="caption">
                            {v.icon &&
                                React.cloneElement(v.icon, {
                                    sx: { height: "24px", mb: "4px", verticalAlign: "middle" },
                                })}
                            {v.label}
                        </Typography>
                        <Typography sx={{ fontWeight: "bold" }}>{v.inner}</Typography>
                    </Box>
                    {i !== values.length - 1 && (
                        <Divider orientation="vertical" variant="middle" flexItem />
                    )}
                </>
            ))}
        </Box>
    );
};

export default LabeledValues;
