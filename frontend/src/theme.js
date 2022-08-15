import { grey, lightGreen, pink, teal } from "@mui/material/colors";
import { createTheme } from "@mui/material/styles";

const theme = createTheme({
    drawerWidth: 240,
    shape: {
        borderRadius: 8,
    },
    palette: {
        primary: {
            main: "#00457E",
        },
        background: {
            default: "#eaeaea",
            panel: "#f0f0f0",
        },
        text: {
            lighter: grey[500],
            lightest: grey[200],
        },
        tool: {
            main: teal[400],
            contrastText: "#fff",
        },
        itemKit: {
            main: lightGreen[300],
            contrastText: "#000",
        },
        qa: {
            main: pink[300],
            contrastText: "#fff",
        },
    },
    components: {
        MuiDivider: {
            styleOverrides: {
                root: {
                    "&.MuiDivider-root::before": {
                        position: "static",
                    },
                    "&.MuiDivider-root::after": {
                        position: "static",
                    },
                },
            },
        },
        MuiDataGrid: {
            styleOverrides: {
                root: {
                    "&.MuiDataGrid-root .MuiDataGrid-toolbarContainer": {
                        paddingTop: 0,
                    },
                    "&.MuiDataGrid-root .MuiDataGrid-columnHeader:focus": {
                        outline: "none",
                    },
                    "&.MuiDataGrid-root .MuiDataGrid-cell:focus": {
                        outline: "none",
                    },
                    "& .datagrid-selectable-row": {
                        cursor: "pointer",
                    },
                },
            },
        },
        MuiCard: {
            styleOverrides: {
                root: {
                    "& .hidden-icon": {
                        display: "none",
                    },
                    "&:hover .hidden-icon": {
                        display: "flex",
                    },
                },
            },
        },
        MuiListItem: {
            styleOverrides: {
                root: {
                    "& .hidden-icon": {
                        display: "none",
                    },
                    "&:hover .hidden-icon": {
                        display: "flex",
                    },
                },
            },
        },
    },
});

export default theme;
