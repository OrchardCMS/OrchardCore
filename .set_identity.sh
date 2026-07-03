if [ "$GIT_AUTHOR_NAME" = "fsfh60" ] || echo "$GIT_AUTHOR_NAME" | grep -qi "fadi" || echo "$GIT_AUTHOR_EMAIL" | grep -qi "fsfh60" || echo "$GIT_COMMITTER_NAME" | grep -qi "fadi" || echo "$GIT_COMMITTER_EMAIL" | grep -qi "fsfh60"; then
  export GIT_AUTHOR_NAME="'+$targetName+'"
  export GIT_AUTHOR_EMAIL="'+$targetEmail+'"
  export GIT_COMMITTER_NAME="'+$targetName+'"
  export GIT_COMMITTER_EMAIL="'+$targetEmail+'"
fi
