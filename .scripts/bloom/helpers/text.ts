const segmenter = new Intl.Segmenter(undefined, { granularity: "grapheme" });

export const textElements = (value: string) => segmenter.segment(value);

export const firstTextElement = (value: string): string => {
    for (const { segment } of textElements(value)) {
        return segment;
    }

    return "";
};

// The limit is a UTF-16 length, not a count of graphemes.
export const truncateText = (value: string, maxLength: number): string => {
    if (maxLength < 0) {
        throw new RangeError("The length limit must not be negative.");
    }

    if (value.length <= maxLength) {
        return value;
    }

    let length = 0;
    for (const { segment, index } of textElements(value)) {
        const end = index + segment.length;
        if (end > maxLength) {
            break;
        }

        length = end;
    }

    return value.slice(0, length);
};
