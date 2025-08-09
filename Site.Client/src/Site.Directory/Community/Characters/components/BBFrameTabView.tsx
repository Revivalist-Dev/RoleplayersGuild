import React from 'react';

interface BBFrameTabViewProps {
    bbFrameHtml: string;
    matureContent: boolean;
    userCanViewMatureContent: boolean;
}

const BBFrameTabView: React.FC<BBFrameTabViewProps> = ({ bbFrameHtml, matureContent, userCanViewMatureContent }) => {
    if (matureContent && !userCanViewMatureContent) {
        return <div className="alert alert-warning">This character's page contains mature content. You can enable mature content in your account settings.</div>;
    }
    return <div dangerouslySetInnerHTML={{ __html: bbFrameHtml }} />;
};

export default BBFrameTabView;
