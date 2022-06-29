import React from 'react';
import {Box, List, ListItem, ListItemText, Paper} from "@mui/material";

const Detectors = ({detectors}) => {
    return (
        <Box sx={{flexGrow: 1, height: 0}}>
            <List sx={{height: '100%', overflowY: 'auto'}}>
                {detectors.map(d => (
                    <ListItem key={d.id}>
                        <ListItemText>{d.name}</ListItemText>
                    </ListItem>
                ))}
            </List>
        </Box>
    );
}

export default Detectors;
