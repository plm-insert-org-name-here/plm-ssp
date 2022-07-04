import { bifilter } from "./utils";

const TEMPLATE_COLOR = "#ff0";
const TEMPLATE_DASH_COLOR = "#fff";
const SELECTED_TEMPLATE_COLOR = "#0f0";
const TEMPLATE_CIRCLE_SIZE = 12;
const TEMPLATE_LINE_WIDTH = 2;

export const canvasActionTypes = {
    none: 0,
    resize: 1,
    move: 2,
};

const drawLabel = (ctx, x, y, text) => {
    ctx.save();

    ctx.globalCompositeOperation = "source-over";
    ctx.setLineDash([]);
    ctx.font = "16px sans-serif";
    ctx.lineWidth = TEMPLATE_LINE_WIDTH;

    const offset_x = 2;
    const offset_y = 4;
    const metrics = ctx.measureText(text);

    ctx.fillStyle = "#000";
    ctx.fillRect(
        x - offset_x,
        y - 2 * offset_y - metrics.actualBoundingBoxAscent,
        metrics.actualBoundingBoxRight + 4 * offset_x,
        metrics.actualBoundingBoxAscent + 2 * offset_y - 1
    );

    ctx.fillStyle = "#fff";
    ctx.fillText(text, x + offset_x, y - offset_y);

    ctx.restore();
};

export const streamDrawFps = (ctx, fps) => {
    ctx.save();

    let color;
    if (fps < 5) {
        color = "#ed371d";
    } else if (fps < 15) {
        color = "#f3ba1c";
    } else {
        color = "#5ef31c";
    }

    ctx.font = "bold 32px sans-serif";
    ctx.fillStyle = color;
    ctx.strokeStyle = "#000";

    ctx.fillText(fps, 4, 28);
    ctx.strokeText(fps, 4, 28);

    ctx.fill();
    ctx.stroke();

    ctx.restore();
};

export const streamDrawTemplates = (ctx, ts, drawLabels) => {
    ctx.save();

    const [tracked, untracked] = bifilter((t) => Object.keys(t).includes("present"), ts);

    ctx.lineWidth = 4;
    ctx.globalAlpha = 0.5;
    untracked.forEach((t) => {
        drawDashedTemplate(ctx, t, TEMPLATE_COLOR, "#444");
        if (drawLabels) drawLabel(ctx, t.x, t.y, t.name);
    });

    ctx.globalAlpha = 1;
    tracked.forEach((t) => {
        drawDashedTemplate(ctx, t, TEMPLATE_COLOR, t.present ? "#0f0" : "#f00");
        if (drawLabels) drawLabel(ctx, t.x, t.y, t.name);
    });

    ctx.restore();
};

export const editorDrawTemplates = (ctx, ts, drawLabels) => {
    ctx.save();

    ctx.lineWidth = TEMPLATE_LINE_WIDTH;

    ts.forEach((t) => {
        drawDashedTemplate(ctx, t, TEMPLATE_COLOR, TEMPLATE_DASH_COLOR);
        if (drawLabels) drawLabel(ctx, t.x, t.y, t.name);
    });

    ctx.restore();
};

const drawDashedTemplate = (ctx, t, bgcolor, dashcolor) => {
    ctx.globalCompositeOperation = "difference";
    ctx.strokeStyle = bgcolor;
    ctx.setLineDash([]);
    ctx.strokeRect(t.x, t.y, t.width, t.height);

    ctx.globalCompositeOperation = "source-over";
    ctx.setLineDash([2 * ctx.lineWidth, 2 * ctx.lineWidth]);
    ctx.strokeStyle = dashcolor;
    ctx.strokeRect(t.x, t.y, t.width, t.height);
};

export const editorDrawSelectedTemplate = (ctx, t, drawLabels) => {
    ctx.save();

    ctx.globalCompositeOperation = "screen";
    ctx.strokeStyle = SELECTED_TEMPLATE_COLOR;
    ctx.lineWidth = TEMPLATE_LINE_WIDTH;
    ctx.setLineDash([]);

    ctx.strokeRect(t.x, t.y, t.width, t.height);

    ctx.arc(t.x + t.width, t.y + t.height, TEMPLATE_CIRCLE_SIZE, 0, 2 * Math.PI);
    ctx.stroke();

    if (drawLabels) drawLabel(ctx, t.x, t.y, t.name);

    ctx.restore();
};

export const editorDarkenOutsideRectangle = (ctx, x, y, w, h, image) => {
    const cw = image.width;
    const ch = image.height;

    ctx.save();

    ctx.globalCompositeOperation = "darken";
    ctx.fillStyle = "rgba(0, 0, 0, 0.2)";

    // top
    ctx.fillRect(0, 0, cw, y);

    // left
    ctx.fillRect(0, y, x, h);

    // right
    ctx.fillRect(x + w, y, cw - x - w, h);

    // bottom
    ctx.fillRect(0, y + h, cw, ch - y - h);

    ctx.restore();
};

export const getActionForTemplate = (x, y, t) => {
    if (isInCircle(x, y, t.x + t.width, t.y + t.height, TEMPLATE_CIRCLE_SIZE)) {
        return { type: canvasActionTypes.resize, args: null };
    } else if (isInRectangle(x, y, t.x, t.y, t.width, t.height)) {
        const offset_x = x - t.x;
        const offset_y = y - t.y;
        return {
            type: canvasActionTypes.move,
            args: { offset_x: offset_x, offset_y: offset_y },
        };
    }

    return null;
};

const isBetween = (x, x1, x2) => {
    return x >= x1 && x <= x2;
};

const distance = (x1, y1, x2, y2) => {
    return Math.sqrt((x1 - x2) ** 2 + (y1 - y2) ** 2);
};

const isInRectangle = (x, y, rx, ry, rw, rh) => {
    return isBetween(x, rx, rx + rw) && isBetween(y, ry, ry + rh);
};

const isInCircle = (x, y, cx, cy, cr) => {
    const d = distance(x, y, cx, cy);
    return d < cr;
};
